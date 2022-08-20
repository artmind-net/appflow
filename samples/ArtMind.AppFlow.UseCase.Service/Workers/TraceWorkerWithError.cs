using ArtMind.AppFlow.Tasks;
using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;
using System;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class TraceWorkerWithError : TraceTask
    {
        private readonly ILogger<TraceWorkerWithError> _logger;
        private readonly ISingletonDependency _singletonDependency;
        private readonly IScopedDependency _scopedDependency;
        private readonly ITransientDependency _transientDependency;

        public TraceWorkerWithError(ILogger<TraceWorkerWithError> logger,
            ISingletonDependency singletonDependency,
            IScopedDependency scopedDependency,
            ITransientDependency transientDependency) :base(logger)
        {
            _logger = logger;
            _singletonDependency = singletonDependency;
            _scopedDependency = scopedDependency;
            _transientDependency = transientDependency;
        }

        protected override void Execute(IAppContext context)
        {
            //_logger.LogInformation(this, context, _singletonDependency, _scopedDependency, _transientDependency);
            throw new Exception("The exception ...");
        }

        //protected override void ExecuteCatch(IAppContext context)
        //{
        //    _logger.LogInformation($"{this} Exception was handled");
        //}
    }
}
