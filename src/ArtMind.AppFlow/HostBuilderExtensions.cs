using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Configuration;

namespace ArtMind.AppFlow
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate)
        {
           return hostBuilder.RegisterServiceFlow(ServiceOptions.Default, configureDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterServiceFlow(ServiceOptions.Default, configureDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceOptions options, Action<IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterServiceFlow(options, configureDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceOptions options, Action<IConfiguration, IAppTaskCollection> configureDelegate)
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
                services.AddSingleton(configureDelegate.ToConfigurableAction());
                services.AddSingleton(options);
                //services.AddSingleton(hostContext.Configuration);
                services.AddTransient<AppFlowHost>();

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<AppFlowHost>());
            });

            return hostBuilder;
        }
    }
}
