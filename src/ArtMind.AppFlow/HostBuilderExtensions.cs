using ArtMind.AppFlow.Abstractions;
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
