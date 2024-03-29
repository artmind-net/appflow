using ArtMind.AppFlow.Tasks;
using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class WhileWorker : TraceTask
    {
        private readonly ILogger<WhileWorker> _logger;
        private readonly ISingletonDependency _singletonDependency;
        private readonly IScopedDependency _scopedDependency;
        private readonly ITransientDependency _transientDependency;

        public WhileWorker(ILogger<WhileWorker> logger,
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
            
            int counter = context.Get<int>(AppSettings.CounterKey);
            _logger.LogInformation($"{this} --- while counter: {counter}");
            
            // while loop condition
            context.Set(AppSettings.CounterKey, ++counter);
        }
    }
}
