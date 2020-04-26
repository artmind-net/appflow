using ArtMind.AppFlow.Abstractions;
using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class InitCounterWorker : BaseWorker, IAppTask
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<InitCounterWorker> _logger;
        private readonly ISingletonDependency _singletonDependency;
        private readonly IScopedDependency _scopedDependency;
        private readonly ITransientDependency _transientDependency;

        public InitCounterWorker(AppSettings appSettings,
            ILogger<InitCounterWorker> logger,
            ISingletonDependency singletonDependency,
            IScopedDependency scopedDependency,
            ITransientDependency transientDependency)
        {
            _appSettings = appSettings;
            _logger = logger;
            _singletonDependency = singletonDependency;
            _scopedDependency = scopedDependency;
            _transientDependency = transientDependency;
        }

        public void Execute(IAppContext context)
        {
            _logger.LogInformation(this, context, _singletonDependency, _scopedDependency, _transientDependency);

            context.Set(AppSettings.CounterKey, _appSettings.CounterStartsAt);
        }
    }
}
