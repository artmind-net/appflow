using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ArtMind.AppFlow
{
    public static class ConsoleAppHostBuilderExtensions
    {
        #region App

        public static IHostBuilder RegisterConsoleFlow(this IHostBuilder hostBuilder, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(ConsoleAppOptions.Default, configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterConsoleFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(ConsoleAppOptions.Default, configureFlowDelegate);
        }

        public static IHostBuilder RegisterConsoleFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(ConsoleAppOptions.Default.ToOptionsFunc(), configureServicesDelegate, configureFlowDelegate);
        }

        public static IHostBuilder RegisterConsoleFlow(this IHostBuilder hostBuilder, ConsoleAppOptions options, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(options.ToOptionsFunc(), configureFlowDelegate);
        }

        public static IHostBuilder RegisterConsoleFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ConsoleAppOptions> optionsDelegate, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(optionsDelegate, configureFlowDelegate.ToConfigurableAction());
        }

        public static IHostBuilder RegisterConsoleFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ConsoleAppOptions> optionsDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(optionsDelegate, null, configureFlowDelegate);
        }

        public static IHostBuilder RegisterConsoleFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ConsoleAppOptions> optionsDelegate, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IAppContext, AppContext>();
                services.AddTransient<ConsoleAppHost>();
                services.AddWorkersTransient(configureFlowDelegate, hostContext.Configuration);

                services.AddSingleton(configureFlowDelegate);
                services.AddSingleton(optionsDelegate);

                configureServicesDelegate?.Invoke(hostContext.Configuration, services); // ??

                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<ConsoleAppHost>());
            });

            return hostBuilder;
        }

        #endregion
    }
}
