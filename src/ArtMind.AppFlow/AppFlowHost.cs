﻿using Microsoft.Extensions.DependencyInjection;
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
            _logger.LogInformation($"{Environment.NewLine}{Environment.NewLine}{this} - running application flow: {++_cycleCounter}");

            _appFlowContext.Clear();

            using (var serviceTaskCollection = AppTaskCollection.CreateRoot(_serviceProvider, stoppingToken, _configureDelegate))
            {
                AppTaskCollectionEngine.Run(serviceTaskCollection, _appFlowContext);
            }

            await Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey}";
        }
    }
}
