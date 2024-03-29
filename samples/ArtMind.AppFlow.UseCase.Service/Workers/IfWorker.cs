using ArtMind.AppFlow.Tasks;
using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class IfWorker : TraceTask
    {
        private readonly ILogger<IfWorker> _logger;
        private readonly ISingletonDependency _singletonDependency;
        private readonly IScopedDependency _scopedDependency;
        private readonly ITransientDependency _transientDependency;

        public IfWorker(ILogger<IfWorker> logger,
            ISingletonDependency singletonDependency,
            IScopedDependency scopedDependency,
            ITransientDependency transientDependency):base(logger)
        {
            _logger = logger;
            _singletonDependency = singletonDependency;
            _scopedDependency = scopedDependency;
            _transientDependency = transientDependency;
        }

        protected override void Execute(IAppContext context)
        {
            //_logger.LogInformation(this, context, _singletonDependency, _scopedDependency, _transientDependency);
            _logger.LogInformation("If worker");
        }
    }
}
