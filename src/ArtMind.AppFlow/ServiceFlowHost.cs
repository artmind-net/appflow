using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    public class ServiceFlowHost : BackgroundService
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private ulong _cycleCounter = 0;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppFlowHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IAppTaskCollection> _configureDelegate;
        private readonly CancellationTokenPropagation _tokenPropagation;

        public ServiceFlowHost(IServiceProvider serviceProvider, Action<IAppTaskCollection> configureDelegate, CancellationTokenPropagation tokenPropagation)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<AppFlowHost>>();
            _appFlowContext = _serviceProvider.GetRequiredService<IAppContext>();
            _configureDelegate = configureDelegate;
            _tokenPropagation = tokenPropagation;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{Environment.NewLine}{Environment.NewLine}{this} - running service flow cycle: {++_cycleCounter}");

                CancellationToken? innerToken = null;
                if (_tokenPropagation == CancellationTokenPropagation.InFlowDepth)
                    innerToken = stoppingToken;

                _appFlowContext.Clear();

                using (var serviceTaskCollection = AppTaskCollection.CreateRoot(_serviceProvider, innerToken, _configureDelegate))
                {
                    AppTaskCollectionEngine.Run(serviceTaskCollection, _appFlowContext);
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
