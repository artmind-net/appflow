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

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExecuteFlowAsync(stoppingToken);
        }

        private Task ExecuteFlowAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                _logger.LogInformation($"{this} >>> AppFlow started ...");

                _appFlowContext.Clear();

                using (var serviceTaskCollection = AppTaskCollection.CreateRoot(_serviceProvider, stoppingToken, _configureDelegate))
                {
                    AppTaskCollectionEngine.Run(serviceTaskCollection, _appFlowContext);
                }

                _logger.LogInformation($"{this} <<< AppFlow finished successfuly.");
            });
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey}";
        }
    }
}
