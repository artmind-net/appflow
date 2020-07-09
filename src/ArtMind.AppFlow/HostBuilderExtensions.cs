using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ArtMind.AppFlow
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate, CancellationTokenPropagation tokenPropagation = CancellationTokenPropagation.AtFlowCycle)
        {
            var serviceFlowConfigureDelegate = configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate));

            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddTransient(di => new ServiceFlowHost(di, configureDelegate, tokenPropagation));

                services.AddHostedService(serviceProvider =>
                {
                    return serviceProvider.GetRequiredService<ServiceFlowHost>();
                });
            });

            return hostBuilder;
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate)
        {
            var serviceFlowConfigureDelegate = configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate));

            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddTransient(di => new AppFlowHost(di, configureDelegate));

                services.AddHostedService(serviceProvider =>
                {
                    return serviceProvider.GetRequiredService<AppFlowHost>();
                });
            });

            return hostBuilder;
        }
    }
}
