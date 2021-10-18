using System;
using Microsoft.Extensions.Configuration;

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

        internal static Action<IConfiguration, IAppTaskCollection> ToConfigurableAction(this Action<IAppTaskCollection> action)
        {
            return (cfg, taskClg) => { action(taskClg); };
        }
    }
}
