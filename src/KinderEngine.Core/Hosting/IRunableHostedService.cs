using Microsoft.Extensions.Hosting;
using System;

namespace KinderEngine.Core.Hosting
{
    public interface IRunableHostedService: IHostedService, IDisposable
    {
        bool IsRunning { get; }
        string ServiceName { get; }
        bool IsAutoRunEnabled { get; }
        bool RunOnStartup { get; }
        int AutoRunDelayDays { get; }
        int AutoRunDelayMinutes { get; }
        int AutoRunDelaySeconds { get; }
        DateTime NextAutoRunUtc { get; }
        void ManualStart();
        void ManualStop();
    }
}
