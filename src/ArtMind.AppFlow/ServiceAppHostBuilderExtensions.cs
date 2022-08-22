using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ArtMind.AppFlow
{
    public static class ServiceAppHostBuilderExtensions
    {
        #region Service

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(ServiceAppOptions.Default.ToOptionsFunc(), configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(ServiceAppOptions.Default.ToOptionsFunc(), configureFlowDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(ServiceAppOptions.Default.ToOptionsFunc(), configureServicesDelegate, configureFlowDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceAppOptions options, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(options.ToOptionsFunc(), configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, ServiceAppOptions options, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(options.ToOptionsFunc(), configureFlowDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceAppOptions> optionsDelegate, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(optionsDelegate, configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceAppOptions> optionsDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterServiceFlow(optionsDelegate, null, configureFlowDelegate);
        }

        public static IHostBuilder RegisterServiceFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ServiceAppOptions> optionsDelegate, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddTransient<ServiceAppHost>();
                services.AddWorkersTransient(configureFlowDelegate, hostContext.Configuration);

                services.AddSingleton(configureFlowDelegate);
                services.AddSingleton(optionsDelegate);

                configureServicesDelegate?.Invoke(hostContext.Configuration, services); // ??

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<ServiceAppHost>());
            });

            return hostBuilder;
        }

        #endregion
    }
}
