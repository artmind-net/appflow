using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    internal class AppFlowHost : BackgroundService
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");

        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppFlowHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IConfiguration, IAppTaskCollection> _configureDelegate;
        private readonly AppOptions _options;

        public AppFlowHost(
            IHostApplicationLifetime appLifetime,
            IServiceProvider serviceProvider,
            Action<IConfiguration, IAppTaskCollection> configureDelegate,
            Func<IConfiguration, AppOptions> optionsDelegate)
        {
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<AppFlowHost>>();
            _appFlowContext = _serviceProvider.GetRequiredService<IAppContext>();
            _options = optionsDelegate?.Invoke(serviceProvider.GetService<IConfiguration>()) ?? AppOptions.Default;
            _configureDelegate = configureDelegate;
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

        #endregion

        private async Task ExecuteFlowAsync(CancellationToken stoppingToken)
        {
            if (_options.ShouldPostpone(out var postpone))
            {
                _logger.LogTrace($"{this} - app flow postponed for {postpone}");
                await Task.Delay(postpone, stoppingToken);
            }

            _logger.LogTrace($"{this} - app flow started at: {DateTimeOffset.Now}");

            _appFlowContext.Clear();

            using (var serviceTaskCollection =
                AppTaskCollection.CreateRoot(_serviceProvider, stoppingToken, _configureDelegate))
            {
                serviceTaskCollection.Run(_appFlowContext);
            }

            _logger.LogTrace($"{this} - app flow finished successfully at: {DateTimeOffset.Now}");
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} [{_instanceKey}]";
        }
    }
}
