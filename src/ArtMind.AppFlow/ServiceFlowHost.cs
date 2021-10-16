using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    public class ServiceFlowHost : BackgroundService
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private ulong _cycleCounter = 0;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppFlowHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IAppTaskCollection> _configureDelegate;
        private readonly IServiceOptions _options;

        public ServiceFlowHost(
            IHostApplicationLifetime appLifetime,
            IServiceProvider serviceProvider,
            IServiceOptions options,
            Action<IAppTaskCollection> configureDelegate)
        {
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<AppFlowHost>>();
            _appFlowContext = _serviceProvider.GetRequiredService<IAppContext>();
            _configureDelegate = configureDelegate;
            _options = options;
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
                _logger.LogError($"{this} - service flow was cancelled.");
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
                while (!stoppingToken.IsCancellationRequested && IsCycleLimitNotExceeded(_cycleCounter))
                {
                    stoppingToken.ThrowIfCancellationRequested(); // check if Ctrl+C pressed 

                    if (HasToDelay(_stopwatch.Elapsed, out var delay))
                    {
                        _logger.LogInformation($"{this} - delaying service flow cycle: {_cycleCounter} for {delay}");
                        Task.Delay(delay, stoppingToken).Wait(stoppingToken);
                    }

                    stoppingToken.ThrowIfCancellationRequested(); // check if Ctrl+C pressed 

                    _stopwatch.Restart();
                    _cycleCounter++;
                    _appFlowContext.Clear();

                    _logger.LogInformation($"{this} - running service flow cycle: {_cycleCounter}");
                    
                    using (var serviceTaskCollection = AppTaskCollection.CreateRoot(_serviceProvider, stoppingToken, _configureDelegate))
                    {
                        serviceTaskCollection.Run(_appFlowContext);
                    }

                    _stopwatch.Stop();

                    _logger.LogInformation($"{this} - ran service flow cycle: {_cycleCounter} in { _stopwatch.Elapsed}");
                }

                if (IsCycleLimitExceeded(_cycleCounter))
                {
                    _logger.LogInformation($"{this} - flow stopped. The service flow reached the cycles limit.");
                }

            }, stoppingToken);
        }

        private bool IsCycleLimitNotExceeded(ulong cycleCounter)
        {
            return _options.CyclesLimit == 0 || cycleCounter < _options.CyclesLimit;
        }

        private bool IsCycleLimitExceeded(ulong cycleCounter)
        {
            return !IsCycleLimitNotExceeded(cycleCounter);
        }

        private bool HasToDelay(TimeSpan duration, out TimeSpan delay)
        {
            delay = TimeSpan.Zero;

            if(duration != TimeSpan.Zero && _options.MinCycleDuration > duration)
                delay = _options.MinCycleDuration - duration;

            return delay != TimeSpan.Zero;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} [{_instanceKey}]";
        }
    }
}
