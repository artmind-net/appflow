using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ArtMind.AppFlow
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterServiceFlow(ServiceOptions.Default, configureDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceOptions options, Action<IAppTaskCollection> configureDelegate)
        {
            var serviceFlowConfigureDelegate = configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate));

            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddSingleton(configureDelegate);
                services.AddSingleton(options);
                services.AddTransient<ServiceFlowHost>();

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<ServiceFlowHost>());
            });

            return hostBuilder;
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterAppFlow(AppOptions.Default, configureDelegate);
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, AppOptions options, Action<IAppTaskCollection> configureDelegate)
        {
            var serviceFlowConfigureDelegate = configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate));

            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddSingleton(configureDelegate);
                services.AddSingleton(options);
                services.AddTransient<AppFlowHost>();

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<AppFlowHost>());
            });

            return hostBuilder;
        }
    }
}
