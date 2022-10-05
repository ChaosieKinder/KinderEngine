using System.Collections.Generic;

namespace KinderEngine.Core.Hosting
{
    public interface IRunableHostedServiceResult<TResult>
    {
        bool Success { get; set; }
        public TResult Result { get; set; }
        List<HostedServiceOperation> Operations { get; }
    }
}
