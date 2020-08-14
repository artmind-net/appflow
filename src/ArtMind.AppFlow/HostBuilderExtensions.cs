using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ArtMind.AppFlow
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate)
        {
            var serviceFlowConfigureDelegate = configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate));

            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddSingleton(configureDelegate);
                services.AddTransient<ServiceFlowHost>();

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
                services.AddSingleton(configureDelegate);
                services.AddTransient<AppFlowHost>();

                services.AddHostedService(serviceProvider =>
                {
                    return serviceProvider.GetRequiredService<AppFlowHost>();
                });
            });

            return hostBuilder;
        }
    }
}
