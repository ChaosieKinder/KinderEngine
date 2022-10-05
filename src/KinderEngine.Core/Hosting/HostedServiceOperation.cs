using System;

namespace KinderEngine.Core.Hosting
{
    public class HostedServiceOperation
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public bool LogOnSuccess { get; set; } = false;
        public Exception Error { get; set; }
        public HostedServiceOperation(string name, string message = "", bool logOnSuccess = true, Exception error = null)
        {
            Name = name;
            Message = message;
            LogOnSuccess = logOnSuccess;
            Error = error;
        }

    }
}
