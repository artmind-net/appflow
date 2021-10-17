﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    internal class ServiceFlowHost : BackgroundService
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private ulong _cycleCounter;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppFlowHost> _logger;
        private readonly IAppContext _appFlowContext;
        private readonly Action<IAppTaskCollection> _configureDelegate;
        private readonly ServiceOptions _options;

        public ServiceFlowHost(
            IHostApplicationLifetime appLifetime,
            IServiceProvider serviceProvider,
            ServiceOptions options,
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

            await Task.Run(() => { }, stoppingToken);
        }

        private Task ExecuteFlow(CancellationToken stoppingToken)
        {
            return Task.Run( async () =>
            {
                _cycleCounter++;

                if (_options.ShouldPostpone(out var postpone))
                {
                    _logger.LogTrace($"{this} - service will start in {postpone}");
                    await Task.Delay(postpone, stoppingToken);
                }

                while (!stoppingToken.IsCancellationRequested && !_options.IsCycleLimitExceeded(_cycleCounter))
                {
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

                    using (var serviceTaskCollection =
                        AppTaskCollection.CreateRoot(_serviceProvider, stoppingToken, _configureDelegate))
                    {
                        serviceTaskCollection.Run(_appFlowContext);
                    }

                    _stopwatch.Stop();

                    _logger.LogTrace($"{this} - ran service flow cycle: {_cycleCounter} in {_stopwatch.Elapsed}");
                }

                if (_options.IsCycleLimitExceeded(_cycleCounter))
                {
                    _logger.LogTrace($"{this} - flow stopped. The service flow reached the occurrence limit.");
                }
            }, stoppingToken);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} [{_instanceKey}]";
        }
    }
}
