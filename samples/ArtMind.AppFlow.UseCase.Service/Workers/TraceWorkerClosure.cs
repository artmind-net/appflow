using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ArtMind.AppFlow.Tasks;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class TraceWorkerClosure : TraceTask
    {
        private readonly ILogger<TraceWorkerClosure> _logger;
        private readonly ISingletonDependency _singletonDependency;
        private readonly IScopedDependency _scopedDependency;
        private readonly ITransientDependency _transientDependency;

        public TraceWorkerClosure(ILogger<TraceWorkerClosure> logger,
            ISingletonDependency singletonDependency,
            IScopedDependency scopedDependency,
            ITransientDependency transientDependency) : base(logger)
        {
            _logger = logger;
            _singletonDependency = singletonDependency;
            _scopedDependency = scopedDependency;
            _transientDependency = transientDependency;
        }

        protected override void Execute(IAppContext context)
        {
           // _logger.LogInformation(this, context, _singletonDependency, _scopedDependency, _transientDependency);
            //throw new Exception("The ex ...");
            _logger.LogInformation("By by i was the last worker in the flow.");
            Task.Delay(1000);
        }
    }
}
