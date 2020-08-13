namespace ArtMind.AppFlow
{
    public static class AppTaskCollectionEngine
    {
        public static void Run(this AppTaskCollection serviceTaskCollection, IAppContext context)
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
    }
}
