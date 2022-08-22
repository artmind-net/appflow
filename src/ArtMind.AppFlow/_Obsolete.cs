using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ArtMind.AppFlow
{
    public static class ObsoleteExtensions
    {
        #region App

        [Obsolete("Use RegisterConsoleFlow(...) instead", false)]
        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(ConsoleAppOptions.Default, configureFlowDelegate.ToConfigurableAction());
        }

        [Obsolete("Use RegisterConsoleFlow(...) instead", false)]
        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(ConsoleAppOptions.Default, configureFlowDelegate);
        }

        [Obsolete("Use RegisterConsoleFlow(...) instead", false)]
        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(ConsoleAppOptions.Default.ToOptionsFunc(), configureServicesDelegate, configureFlowDelegate);
        }

        [Obsolete("Use RegisterConsoleFlow(...) instead", false)]
        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, ConsoleAppOptions options, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(options.ToOptionsFunc(), configureFlowDelegate);
        }

        [Obsolete("Use RegisterConsoleFlow(...) instead", false)]
        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ConsoleAppOptions> optionsDelegate, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(optionsDelegate, configureFlowDelegate.ToConfigurableAction());
        }

        [Obsolete("Use RegisterConsoleFlow(...) instead", false)]
        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ConsoleAppOptions> optionsDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(optionsDelegate, null, configureFlowDelegate);
        }

        [Obsolete("Use RegisterConsoleFlow(...) instead", false)]
        public static IHostBuilder RegisterAppFlow(this IHostBuilder hostBuilder, Func<IConfiguration, ConsoleAppOptions> optionsDelegate, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterConsoleFlow(optionsDelegate, configureServicesDelegate, configureFlowDelegate);
        }

        #endregion
    }
}
