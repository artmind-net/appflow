using ArtMind.AppFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ArtMind.AppFlow
{
    public delegate void AppTaskResolver(IAppContext ctx);

    public class AppTaskCollection : IAppTaskCollection, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly IServiceScope _serviceScope;
        //private readonly ILogger<AppTaskCollection> _logger;

        private bool HasDisposableScope { get; set; }
        public List<AppTaskResolver> ServiceTaskResolvers { get; } = new List<AppTaskResolver>();

        private AppTaskCollection(IServiceScope serviceScope, Action<IAppTaskCollection> configureDelegate, bool useInnerScope)
        {
            HasDisposableScope = useInnerScope;
            _serviceScope = useInnerScope ? serviceScope.ServiceProvider.CreateScope() : serviceScope;
            //_logger = _serviceScope.ServiceProvider.GetRequiredService<ILogger<AppTaskCollection>>();

            // _logger.LogInformation($"{this} - Creating ...");

            configureDelegate(this);

            //_logger.LogInformation($"{this} - Created.");
        }

        public AppTaskCollection(IServiceProvider serviceProvider, Action<IAppTaskCollection> configureDelegate)
            : this(serviceProvider.CreateScope(), configureDelegate, false)
        {
            this.HasDisposableScope = true;
        }

        public static AppTaskCollection CreateRoot(IServiceProvider serviceProvider, Action<IAppTaskCollection> configureDelegate)
        {
            return new AppTaskCollection(serviceProvider, configureDelegate);
        }

        public void Dispose()
        {
            //_logger.LogInformation($"{this} - Disposing ...");

            ServiceTaskResolvers.Clear();

            if (HasDisposableScope)
                _serviceScope.Dispose();

            //_logger.LogInformation($"{this} - Disposed.");
        }

        #region IAppTaskCollection

        public IAppTaskCollection UseAppTask(Action<IAppContext> appTaskDelegate)
        {
            //_logger.LogInformation($"{this} - UseAppTask.");

            ServiceTaskResolvers.Add(ctx => { appTaskDelegate(ctx); });

            return this;
        }

        public IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : IAppTask
        {
            //_logger.LogInformation($"{this} - UseAppTask<>.");

            void resolver(IAppContext ctx)
            {
                var serticeTask = _serviceScope.ServiceProvider.GetRequiredService<TAppTask>();
                serticeTask.Execute(ctx);
            }

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            //_logger.LogInformation($"{this} - UseIfBranch.");

            void resolver(IAppContext ctx)
            {
                if (predicate(ctx) == false)
                    return;

                using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, branchFlow, createNestedScope))
                {
                    //_logger.LogInformation($"{this} - executing collection: {serviceTaskCollection} ...");
                    AppTaskCollectionEngine.Run(serviceTaskCollection, ctx);
                    //_logger.LogInformation($"{this} - executed collection: {serviceTaskCollection}.");
                }
            }

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            //_logger.LogInformation($"{this} - AddBranchWhile.");

            void resolver(IAppContext ctx)
            {
                while (predicate(ctx))
                {
                    using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, branchFlow, createNestedScope))
                    {
                        //_logger.LogInformation($"{this} - executing collection: {serviceTaskCollection} ...");
                        AppTaskCollectionEngine.Run(serviceTaskCollection, ctx);
                        //_logger.LogInformation($"{this} - executed collection: {serviceTaskCollection}.");
                    }
                }
            };

            return UseAppTask(resolver);
        }

        #endregion

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey} [{ServiceTaskResolvers.Count}]";
        }
    }
}
