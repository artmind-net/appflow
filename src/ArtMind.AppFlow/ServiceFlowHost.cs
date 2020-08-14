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

        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppFlowHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IAppTaskCollection> _configureDelegate;

        public ServiceFlowHost(IHostApplicationLifetime appLifetime, IServiceProvider serviceProvider, Action<IAppTaskCollection> configureDelegate)
        {
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<AppFlowHost>>();
            _appFlowContext = _serviceProvider.GetRequiredService<IAppContext>();
            _configureDelegate = configureDelegate;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await ExecuteFlow(stoppingToken);

                // see if the user pressed Ctrl+C
                stoppingToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                _logger.LogError($"{this} - servce flow was cancelled.");
                Environment.ExitCode = 2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this} - service flow failed.");
                Environment.ExitCode = 1;
            }
            finally
            {
                // No matter what happens (success or exception), we need to indicate that it's time to stop the application.
                _appLifetime.StopApplication();
            }


            await Task.Run(() => {  }, stoppingToken);
        }

        private Task ExecuteFlow(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // see if the user pressed Ctrl+C
                    stoppingToken.ThrowIfCancellationRequested();
                    _logger.LogInformation("{self} - running service flow cycle: {cycleCounter}", this, ++_cycleCounter);
                    _appFlowContext.Clear();
                    using (var serviceTaskCollection = AppTaskCollection.CreateRoot(_serviceProvider, stoppingToken, _configureDelegate))
                    {
                        serviceTaskCollection.Run(_appFlowContext);
                    }
                }
            });
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} [{_instanceKey}]";
        }
    }
}
