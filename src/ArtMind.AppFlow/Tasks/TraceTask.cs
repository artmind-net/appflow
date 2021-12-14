using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace ArtMind.AppFlow.Tasks
{
    public abstract class TraceTask : IAppTask
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        protected ILogger Logger { get; private set; }

        protected TraceTask(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected abstract void Execute(IAppContext context);

        void IAppTask.Execute(IAppContext context)
        {
            Logger.LogTrace($"{this} - task execution started");

            var sw = Stopwatch.StartNew();
            try
            {
                Execute(context);
            }
            catch (Exception ex)
            {
                sw.Stop();
                Logger.LogTrace($"{this} - task execution failed in {sw.Elapsed}");
                throw new Exception(ex.Message, ex);
            }

            sw.Stop();
            Logger.LogTrace($"{this} - task execution finished in {sw.Elapsed}");
        }

        public override string ToString()
        {
            return $"{this.GetType().GetFriendlyName()} [{_instanceKey}]";
        }
    }
}
