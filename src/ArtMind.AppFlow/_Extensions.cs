using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtMind.AppFlow
{
    internal static class Extensions
    {
        internal static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = typeParameters[i].GetFriendlyName();
                    friendlyName += i == 0 ? typeParamName : "," + typeParamName;
                }
                friendlyName += ">";
            }

            return friendlyName;
        }

        internal static Action<IConfiguration, IAppFlowBuilder> ToConfigurableAction(this Action<IAppFlowBuilder> action)
        {
            return (cfg, taskClg) => { action(taskClg); };
        }

        internal static Func<IConfiguration, ServiceAppOptions> ToOptionsFunc(this ServiceAppOptions options)
        {
            return cfg => options;
        }

        internal static Func<IConfiguration, ConsoleAppOptions> ToOptionsFunc(this ConsoleAppOptions options)
        {
            return cfg => options;
        }

        internal static void AddWorkersTransient(this IServiceCollection services, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate, IConfiguration configuration)
        {
            var workerTypes = AppFlowBuilder.GetWorkerTypes(configureFlowDelegate, configuration);

            foreach (var type in workerTypes)
            {
                if (services != null && !services.Any(x => x.ServiceType == type))
                {
                    services.AddTransient(type);
                }
            }
        }

        internal static void AddWorkersScoped(this IServiceCollection services, Action<IConfiguration, IAppFlowBuilder> configureFlowDelegate, IConfiguration configuration)
        {
            var workerTypes = AppFlowBuilder.GetWorkerTypes(configureFlowDelegate, configuration);

            foreach (var type in workerTypes)
            {
                if (services != null && !services.Any(x => x.ServiceType == type))
                {
                    services.AddScoped(type);
                }
            }
        }
    }
}
