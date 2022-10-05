using System;
using System.Collections.Generic;

namespace KinderEngine.Core.Hosting
{ 
    public class RunableHostedServiceResult<TResult> : IRunableHostedServiceResult<TResult>
    {
        public bool Success { get; set; } = false;
        public TResult Result { get; set; } = default(TResult);
        public List<HostedServiceOperation> Operations { get; } = new List<HostedServiceOperation>();
    }
}
