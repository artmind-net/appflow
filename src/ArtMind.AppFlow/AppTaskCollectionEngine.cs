using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    internal static class AppTaskCollectionEngine
    {
        internal static void Run(this AppTaskCollection appTaskCollection, IAppContext context)
        {
            if (appTaskCollection.IsCancellationRequested)
                return;

            foreach (var taskResolver in appTaskCollection.ServiceAppTaskResolvers)
            {
                if (appTaskCollection.IsCancellationRequested)
                    break;
                
                try
                {
                    taskResolver
                        .Invoke()
                        .Invoke(context);
                }
                catch(Exception ex)
                {
                    // TODO: check error handling
                     throw new Exception(ex.Message, ex);
                    // throw ex;
                }
            }
        }

        internal static async Task RunAsync(this AppTaskCollection serviceTaskCollection, IAppContext context, CancellationToken stoppingToke)
        {
            if (serviceTaskCollection.IsCancellationRequested)
                return;

            foreach (var taskResolver in serviceTaskCollection.ServiceAppTaskResolvers)
            {
                if (serviceTaskCollection.IsCancellationRequested)
                    break;

                await Task.Run(() =>
                {
                    try
                    {
                        taskResolver
                              .Invoke()
                              .Invoke(context);
                    }
                    catch(Exception ex)
                    {
                        if (ex.InnerException != null)
                            ex = ex.InnerException;

                        throw new Exception(ex.Message, ex);
                    }
                });
            }
        }
    }
}
