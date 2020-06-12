using ArtMind.AppFlow.Abstractions;
using System.Linq;

namespace ArtMind.AppFlow
{
    public class AppTaskCollectionEngine
    {
        public static void Run(AppTaskCollection serviceTaskCollection, IAppContext context)
        {
            if (serviceTaskCollection.IsCancellationRequested)
                return;

            var appTasks = serviceTaskCollection.ServiceTaskResolvers.ToList();
            foreach (var serviceTask in appTasks)
            {
                if (serviceTaskCollection.IsCancellationRequested)
                    break;

                serviceTask(context);
            }
        }
    }
}
