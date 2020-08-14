using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;
using System;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class ExWorker : BaseWorker, IAppTask
    {
        private readonly ILogger<FinishWorker> _logger;
        private readonly ISingletonDependency _singletonDependency;
        private readonly IScopedDependency _scopedDependency;
        private readonly ITransientDependency _transientDependency;

        public ExWorker(ILogger<FinishWorker> logger,
            ISingletonDependency singletonDependency,
            IScopedDependency scopedDependency,
            ITransientDependency transientDependency)
        {
            _logger = logger;
            _singletonDependency = singletonDependency;
            _scopedDependency = scopedDependency;
            _transientDependency = transientDependency;
        }

        public void Execute(IAppContext context)
        {
            _logger.LogInformation(this, context, _singletonDependency, _scopedDependency, _transientDependency);
            throw new Exception("The ex ...");
        }
    }
}
