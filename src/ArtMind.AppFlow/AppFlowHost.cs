using ArtMind.AppFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    public class AppFlowHost : BackgroundService
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private ulong _cycleCounter = 0;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppFlowHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IAppTaskCollection> _configureDelegate;

        public AppFlowHost(IServiceProvider serviceProvider, Action<IAppTaskCollection> configureDelegate)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<AppFlowHost>>();
            _appFlowContext = _serviceProvider.GetRequiredService<IAppContext>();
            _configureDelegate = configureDelegate;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{Environment.NewLine}{Environment.NewLine}{this} - running service flow cycle: {++_cycleCounter}");

                _appFlowContext.Clear();

                using (var serviceTaskCollection = AppTaskCollection.CreateRoot(_serviceProvider, _configureDelegate))
                {
                    //_logger.LogInformation($"{this} - executing collection: {serviceTaskCollection} ...");
                    AppTaskCollectionEngine.Run(serviceTaskCollection, _appFlowContext);
                    // _logger.LogInformation($"{this} - executed collection: {serviceTaskCollection}.");
                }
            }

            await Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey}";
        }
    }
}
