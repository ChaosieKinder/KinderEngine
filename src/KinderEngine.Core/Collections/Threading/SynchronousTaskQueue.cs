using System;
using System.Threading;
using System.Threading.Tasks;

namespace KinderEngine.Core.Collections.Threading
{
    /// <summary>
    ///     Creates a thread/queue that executes an action on queued objects FIFO on a Task
    ///     leverages AsyncQueue as underlying queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SynchronousTaskQueue<T> : IExecutionQueue<T>
    {
        private readonly AsyncQueue<T> _jobs = new AsyncQueue<T>();
        private readonly Func<T, Task> _processAction;

        public SynchronousTaskQueue(Func<T, Task> workerMethod)
        {
            _processAction = workerMethod;
            Task.Run(ProcessQueuedItems);
        }

        public void Enqueue(T job)
            => _jobs.Enqueue(job);

        private async Task ProcessQueuedItems()
        {
            await foreach (T i in _jobs)
            {
                // Writes a line as soon as some other Task calls queue.Enqueue(..)
                await _processAction.Invoke(i);
            }
        }
    }
}
