using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace ArtMind.AppFlow.Tasks
{
    public abstract class SafeTask : IAppTask
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        protected ILogger Logger { get; private set; }

        protected SafeTask(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected abstract void Execute(IAppContext context);

        protected virtual void ExecuteCatch(IAppContext context) { }

        void IAppTask.Execute(IAppContext context)
        {
            try
            {
                Logger.LogTrace($"{this} - task execution started");

                var sw = Stopwatch.StartNew();
                Execute(context);
                sw.Stop();

                Logger.LogTrace($"{this} - task execution finished in  {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{this} - task failed.");
                ExecuteCatch(context);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().GetFriendlyName()} [{_instanceKey}]";
        }
    }
}
