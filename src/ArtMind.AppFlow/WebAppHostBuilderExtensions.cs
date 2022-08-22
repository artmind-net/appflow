using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ArtMind.AppFlow
{
    public static class WebAppHostBuilderExtensions
    {
        #region WebApplication

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterWebFlow(ServiceAppOptions.Default.ToOptionsFunc(), configureFlowDelegate.ToConfigurableAction());
        }

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterWebFlow(ServiceAppOptions.Default.ToOptionsFunc(), configureFlowDelegate);
        }

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterWebFlow(ServiceAppOptions.Default.ToOptionsFunc(), configureServicesDelegate, configureFlowDelegate);
        }

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, ServiceAppOptions options, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterWebFlow(options.ToOptionsFunc(), configureFlowDelegate.ToConfigurableAction());
        }

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, ServiceAppOptions options, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterWebFlow(options.ToOptionsFunc(), configureFlowDelegate);
        }

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, Func<IConfiguration, ServiceAppOptions> optionsDelegate, Action<IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterWebFlow(optionsDelegate, configureFlowDelegate.ToConfigurableAction());
        }

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, Func<IConfiguration, ServiceAppOptions> optionsDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            return hostBuilder.RegisterWebFlow(optionsDelegate, null, configureFlowDelegate);
        }

        public static WebApplicationBuilder RegisterWebFlow(this WebApplicationBuilder hostBuilder, Func<IConfiguration, ServiceAppOptions> optionsDelegate, Action<IConfiguration, IServiceCollection> configureServicesDelegate, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate)
        {
            var services = hostBuilder.Services;

            services.AddSingleton<IAppContext, AppContext>();
            services.AddTransient<ServiceAppHost>();
            services.AddWorkersTransient(configureFlowDelegate, hostBuilder.Configuration);

            services.AddSingleton(configureFlowDelegate);
            services.AddSingleton(optionsDelegate);

            configureServicesDelegate?.Invoke(hostBuilder.Configuration, hostBuilder.Services);

            hostBuilder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<ServiceAppHost>());

            return hostBuilder;
        }

        #endregion
    }
}
