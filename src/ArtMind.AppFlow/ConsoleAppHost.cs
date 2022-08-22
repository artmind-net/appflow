using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    internal class ConsoleAppHost : BackgroundService
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConsoleAppHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IConfiguration, IAppFlowBuilder> _configureDelegate;
        private readonly ConsoleAppOptions _options;

        public ConsoleAppHost(
            IHostApplicationLifetime appLifetime,
            IServiceProvider serviceProvider,
            Action<IConfiguration, IAppFlowBuilder> configureDelegate,
            Func<IConfiguration, ConsoleAppOptions> optionsDelegate)
        {
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;           
            _configureDelegate = configureDelegate;
            _options = optionsDelegate?.Invoke(serviceProvider.GetService<IConfiguration>()) ?? ConsoleAppOptions.Default;

            _logger = _serviceProvider.GetRequiredService<ILogger<ConsoleAppHost>>();
            _appFlowContext = _serviceProvider.GetRequiredService<IAppContext>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_appLifetime != null)
            {
                _appLifetime.ApplicationStarted.Register(() =>
                {
                    Task.Run(async () => await Execution(stoppingToken));
                });
            }
            else
            {
                await Execution(stoppingToken);
            }
        }

        #region Helpers

        private async Task Execution(CancellationToken stoppingToken)
        {
            try
            {
                await ExecuteFlowAsync(stoppingToken);

                // see if the user pressed Ctrl+C
                stoppingToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                _logger.LogError($"{this} - app flow  was cancelled.");
                Environment.ExitCode = 2;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    ex = ex.InnerException;

                _logger.LogError(ex, $"{this} - app flow failed.");
                Environment.ExitCode = 1;
            }
            finally
            {
                // No matter what happens (success or exception), we need to indicate that it's time to stop the application.
                _appLifetime.StopApplication();
            }
        }

        private async Task ExecuteFlowAsync(CancellationToken stoppingToken)
        {
            if (_options.ShouldPostpone(out var postpone))
            {
                _logger.LogTrace($"{this} - app flow postponed for {postpone}");
                await Task.Delay(postpone, stoppingToken);
            }

            _logger.LogTrace($"{this} - app flow started at: {DateTimeOffset.Now}");

            _appFlowContext.Clear();

            using (var serviceTaskCollection = AppFlowBuilder.CreateRoot(stoppingToken, _configureDelegate, _serviceProvider))
            {
                serviceTaskCollection.Run(_appFlowContext);
            }

            _logger.LogTrace($"{this} - app flow finished successfully at: {DateTimeOffset.Now}");
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} [{_instanceKey}]";
        }

        #endregion
    }
}
