using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ArtMind.AppFlow
{
    public static class HostBuilderExtensions
    {
        #region Service

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureFlowDelegate)
        {
           return hostBuilder.RegisterServiceFlow(ServiceOptions.Default.ToOptionsFunc(), configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(ServiceOptions.Default.ToOptionsFunc(), configureFlowDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceOptions options, Action<IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(options.ToOptionsFunc(), configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceOptions options, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(options.ToOptionsFunc(), configureFlowDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceOptions> optionsDelegate, Action<IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(optionsDelegate, configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceOptions> optionsDelegate, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(optionsDelegate, null, configureFlowDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceOptions> optionsDelegate, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddSingleton(configureFlowDelegate);
                services.AddSingleton(optionsDelegate);
                services.AddSingleton(services);
                services.AddTransient<ServiceFlowHost>();
                
                configureServicesDelegate?.Invoke(hostContext.Configuration, services);

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<ServiceFlowHost>());
            });

            return hostBuilder;
        }

        #endregion

        #region App

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterAppFlow(AppOptions.Default, configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterAppFlow(AppOptions.Default, configureFlowDelegate);
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, AppOptions options, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterAppFlow(options.ToOptionsFunc(), configureFlowDelegate);
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, AppOptions> optionsDelegate, Action<IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterAppFlow(optionsDelegate, configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, AppOptions> optionsDelegate, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            return hostBuilder.RegisterAppFlow(optionsDelegate, null, configureFlowDelegate);
        }

        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, AppOptions> optionsDelegate, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppTaskCollection> configureFlowDelegate)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddSingleton(configureFlowDelegate);
                services.AddSingleton(optionsDelegate);
                services.AddSingleton(services);
                services.AddTransient<AppFlowHost>();

                configureServicesDelegate?.Invoke(hostContext.Configuration, services);

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<AppFlowHost>());
            });

            return hostBuilder;
        }

        #endregion
    }
}
