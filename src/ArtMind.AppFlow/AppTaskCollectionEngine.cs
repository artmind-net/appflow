using ArtMind.AppFlow.Abstractions;
using System.Linq;

namespace ArtMind.AppFlow
{
    public class AppTaskCollectionEngine
    {
        public static void Run(AppTaskCollection serviceTaskCollection, IAppContext context)
        {
            var x = serviceTaskCollection.ServiceTaskResolvers.ToList();
            foreach (var serviceTask in x)
            {
                serviceTask(context);
            }
        }
    }
}
