using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    internal static class AppFlowEngine
    {
        internal static void Run(this AppFlowBuilder appFlowBuilder, IAppContext context)
        {
            if (appFlowBuilder.IsCancellationRequested)
                return;

            foreach (var taskResolver in appFlowBuilder.ServiceAppTaskResolvers)
            {
                if (appFlowBuilder.IsCancellationRequested)
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

        internal static async Task RunAsync(this AppFlowBuilder appFlowBuilder, IAppContext context, CancellationToken stoppingToke)
        {
            if (appFlowBuilder.IsCancellationRequested)
                return;

            foreach (var taskResolver in appFlowBuilder.ServiceAppTaskResolvers)
            {
                if (appFlowBuilder.IsCancellationRequested)
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
