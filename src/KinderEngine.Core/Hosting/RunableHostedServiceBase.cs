using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KinderEngine.Core.Hosting
{
    /// <summary>
    ///     Should maintain a queue of tokendb pages to scan
    /// </summary>
    public abstract class RunableHostedServiceBase<TResult> : IRunableHostedService<TResult>
    {
        public bool IsRunning { get; protected set; }
        public virtual string ServiceName { get => $"Unnamed_{nameof(RunableHostedServiceBase<TResult>)}"; }
        public virtual bool IsAutoRunEnabled { get => false; }
        public virtual bool RunOnStartup { get => false; }
        public virtual int AutoRunDelayDays { get => 0; }
        public virtual int AutoRunDelayHours { get => 0; }
        public virtual int AutoRunDelayMinutes { get => 0; }
        public virtual int AutoRunDelaySeconds { get => 60; }
        public DateTime NextAutoRunUtc { get; protected set; } = DateTime.MinValue;

        private CancellationTokenSource _autoScrapeDelayTaskCancelationToken;
        private CancellationTokenSource _workerRunningCancelationTokenSource;
        private Task _autoScrapeDelayTask;
        private bool _isInitialStartup = true;
        private bool _shouldNotQuit = true;
        private TaskCompletionSource<bool> _workerPendingCompletionSource;
        private object _lock = new object();
        protected IConfiguration _configuration;
        protected ILogger _logger;
        protected abstract Task<IRunableHostedServiceResult<TResult>> Run(IRunableHostedServiceResult<TResult> result, CancellationToken token);

        public RunableHostedServiceBase(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        #region Public Actions
        public void ManualStart() => _workerTriggerStart();
        public void ManualStop() => _workerTriggerStop();
        protected virtual async Task AfterRun(IRunableHostedServiceResult<TResult> result)
        {
            _logger.LogInformation($"{ServiceName} worker finished with success={result.Success}");
            StringBuilder sb = new StringBuilder();
            foreach (var op in result.Operations)
            {
                if ((result.Success && !op.LogOnSuccess) || !result.Success) continue;
                if (string.IsNullOrEmpty(op.Message))
                    sb.Append($"\tOperation: {op.Name}: {op.Message}");
                if (op.Error is not null)
                    sb.Append($"\t{op.Error.ToString()}");
            }
            _logger.LogInformation(sb.ToString());
        }
        #endregion

        protected virtual void _workerReset()
        {
            _shouldNotQuit = true;
            IsRunning = false;
            _workerPendingCompletionSource = new TaskCompletionSource<bool>();
            _workerRunningCancelationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Called when its time to start the worker
        /// </summary>
        protected virtual void _workerTriggerStart()
        {
            if (IsRunning) return; // early exit if we can
            bool weSet = false;
            lock (_lock)
            {
                if (!IsRunning)
                    IsRunning = true;
                weSet = true;
            }
            if (!weSet) return; // early-late exit point :)
            _workerPendingCompletionSource.SetResult(true);
        }

        protected virtual void _workerTriggerStop()
        {
            _workerRunningCancelationTokenSource.Cancel();
        }
        protected virtual void _workerComplete()
        {
            IsRunning = false;
            _setupAutoScrape();
        }

        protected virtual void _setupAutoScrape()
        {
            if (!IsAutoRunEnabled) return;

            TimeSpan nextRun = new TimeSpan(AutoRunDelayDays, AutoRunDelayHours, AutoRunDelayMinutes, AutoRunDelaySeconds, 0);
            NextAutoRunUtc = DateTime.UtcNow.Add(nextRun);
            _autoScrapeDelayTaskCancelationToken = new CancellationTokenSource();
            _autoScrapeDelayTask = Task.Delay(nextRun, _autoScrapeDelayTaskCancelationToken.Token);
            _autoScrapeDelayTask.ContinueWith((o) => { _autoScrapeTimerExpired(o); }, _autoScrapeDelayTaskCancelationToken.Token);
        }

        protected virtual void _autoScrapeTimerExpired(Task previousTask)
        {
            if (previousTask.IsCanceled) return;
            _workerTriggerStart();
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_isInitialStartup)
                _setupAutoScrape();
            _ = Task.Run(_backgroundTaskRunner, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task _backgroundTaskRunner()
        {
            while (_shouldNotQuit)
            {
                // Initialize default pre-run state
                _workerReset();
                // Wait for 
                if (RunOnStartup && _isInitialStartup)
                {
                    _isInitialStartup = false;
                }
                else
                {
                    _isInitialStartup = false;
                    bool continueTask = await _workerPendingCompletionSource.Task;
                    if (!continueTask) return;
                }
                _workerTriggerStart();
                var result = await Run(new RunableHostedServiceResult<TResult>(), _workerRunningCancelationTokenSource.Token);
                await AfterRun(result);

                _workerComplete();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _shouldNotQuit = false;
            _workerPendingCompletionSource.SetResult(false);
            _autoScrapeDelayTaskCancelationToken.Cancel();
        }

        public void Dispose()
        {
            if(_autoScrapeDelayTask is not null && _autoScrapeDelayTask.IsCompleted)
                _autoScrapeDelayTask?.Dispose();
            _autoScrapeDelayTaskCancelationToken?.Dispose();
        }

    }
}
