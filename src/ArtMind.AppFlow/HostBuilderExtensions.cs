using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Configuration;

namespace ArtMind.AppFlow
{
    public static class HostBuilderExtensions
    {
        #region Service

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate)
        {
           return hostBuilder.RegisterServiceFlow(ServiceOptions.Default.ToOptionsFunc(), configureDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterServiceFlow(ServiceOptions.Default.ToOptionsFunc(), configureDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceOptions options, Action<IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterServiceFlow(options.ToOptionsFunc(), configureDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceOptions options, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterServiceFlow(options.ToOptionsFunc(), configureDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceOptions> optionsDelegate, Action<IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterServiceFlow(optionsDelegate, configureDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceOptions> optionsDelegate, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddSingleton(configureDelegate);
                services.AddSingleton(optionsDelegate);
                services.AddTransient<ServiceFlowHost>();

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<ServiceFlowHost>());
            });

            return hostBuilder;
        }

        #endregion

        #region App

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterAppFlow(AppOptions.Default, configureDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterAppFlow(AppOptions.Default, configureDelegate);
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, AppOptions options, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterAppFlow(options.ToOptionsFunc(), configureDelegate);
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, AppOptions> optionsDelegate, Action<IAppTaskCollection> configureDelegate)
        {
            return hostBuilder.RegisterAppFlow(optionsDelegate, configureDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, AppOptions> optionsDelegate, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddSingleton(configureDelegate);
                services.AddSingleton(optionsDelegate);
                //services.AddSingleton(hostContext.Configuration);
                services.AddTransient<AppFlowHost>();

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<AppFlowHost>());
            });

            return hostBuilder;
        }

        #endregion
    }
}
