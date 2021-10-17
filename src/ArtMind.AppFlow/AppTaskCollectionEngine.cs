namespace ArtMind.AppFlow
{
    internal static class AppTaskCollectionEngine
    {
        internal static void Run(this AppTaskCollection serviceTaskCollection, IAppContext context)
        {
            if (serviceTaskCollection.IsCancellationRequested)
                return;

            foreach (var taskResolver in serviceTaskCollection.ServiceAppTaskResolvers)
            {
                if (serviceTaskCollection.IsCancellationRequested)
                    break;

                taskResolver
                    .Invoke()
                    .Invoke(context);
            }
        }

        //internal static async Task RunAsync(this AppTaskCollection serviceTaskCollection, IAppContext context, CancellationToken stoppingToke)
        //{
        //    if (serviceTaskCollection.IsCancellationRequested)
        //        return;

        //    foreach (var taskResolver in serviceTaskCollection.ServiceAppTaskResolvers)
        //    {
        //        if (serviceTaskCollection.IsCancellationRequested)
        //            break;

        //        taskResolver
        //            .Invoke()
        //            .Invoke(context);
        //    }
        //}
    }
}
