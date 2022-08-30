using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    internal class ServiceAppHost : BackgroundService
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private ulong _cycleCounter;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConsoleAppHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IConfiguration, IAppFlowBuilder> _configureDelegate;        
        private readonly ServiceAppOptions _options;

        public ServiceAppHost(
            IHostApplicationLifetime appLifetime,
            IServiceProvider serviceProvider,
            Func<IConfiguration, ServiceAppOptions> optionsDelegate,
            Action<IConfiguration, IAppFlowBuilder> configureDelegate)
        {
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;            
            _configureDelegate = configureDelegate;
            _options = optionsDelegate?.Invoke(serviceProvider.GetService<IConfiguration>()) ?? ServiceAppOptions.Default;

            _logger = _serviceProvider.GetRequiredService<ILogger<ConsoleAppHost>>();
            _appFlowContext = _serviceProvider.GetRequiredService<IAppContext>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await ExecuteFlowAsync(stoppingToken);

                // see if the user pressed Ctrl+C
                stoppingToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                _logger.LogError($"{this} - service flow was cancelled.");
                Environment.ExitCode = 2;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    ex = ex.InnerException;

                _logger.LogError(ex, $"{this} - service flow failed.");
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
                _logger.LogTrace($"{this} - service will start in {postpone}");
                await Task.Delay(postpone, stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested && !_options.IsCycleLimitExceeded(_cycleCounter))
            {
                _cycleCounter++;

                stoppingToken.ThrowIfCancellationRequested(); // check if Ctrl+C pressed 

                if (_options.ShouldDelay(_stopwatch.Elapsed, out TimeSpan delay))
                {
                    _logger.LogTrace($"{this} - delaying service flow cycle: {_cycleCounter} for {delay}");
                    await Task.Delay(delay, stoppingToken);
                }

                stoppingToken.ThrowIfCancellationRequested(); // check if Ctrl+C pressed 

                _stopwatch.Restart();
                _appFlowContext.Clear();

                _logger.LogTrace($"{this} - running service flow cycle: {_cycleCounter}");

                using (var serviceTaskCollection = AppFlowBuilder.CreateRoot(stoppingToken, _configureDelegate, _serviceProvider))
                {
                    try
                    {
                        await serviceTaskCollection.RunAsync(_appFlowContext, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                            ex = ex.InnerException;

                        _logger.LogError(ex, $"Service flow cycle: {_cycleCounter} failed.");
                    }
                }

                _stopwatch.Stop();

                _logger.LogTrace($"{this} - ran service flow cycle: {_cycleCounter} in {_stopwatch.Elapsed}");
            }

            if (_options.IsCycleLimitExceeded(_cycleCounter))
            {
                _logger.LogTrace($"{this} - flow stopped. The service flow reached the occurrence limit.");
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} [{_instanceKey}]";
        }
    }
}
