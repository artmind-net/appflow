using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ArtMind.AppFlow
{
    public delegate void AppTaskResolver(IAppContext ctx);

    public class AppTaskCollection : IAppTaskCollection, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly IServiceScope _serviceScope;
        private readonly CancellationToken? _stoppingToken;

        public bool IsCancellationRequested { get => _stoppingToken.HasValue ? _stoppingToken.Value.IsCancellationRequested : false; }

        private bool HasDisposableScope { get; set; }
        public List<AppTaskResolver> ServiceTaskResolvers { get; } = new List<AppTaskResolver>();

        private AppTaskCollection(IServiceScope serviceScope, CancellationToken? stoppingToken, Action<IAppTaskCollection> configureDelegate, bool useInnerScope)
        {
            HasDisposableScope = useInnerScope;
            _serviceScope = useInnerScope ? serviceScope.ServiceProvider.CreateScope() : serviceScope;
            _stoppingToken = stoppingToken;

            configureDelegate(this);
        }

        public AppTaskCollection(IServiceProvider serviceProvider, CancellationToken? stoppingToken, Action<IAppTaskCollection> configureDelegate)
            : this(serviceProvider.CreateScope(), stoppingToken, configureDelegate, false)
        {
            this.HasDisposableScope = true;
        }

        public static AppTaskCollection CreateRoot(IServiceProvider serviceProvider, CancellationToken? stoppingToken, Action<IAppTaskCollection> configureDelegate)
        {
            return new AppTaskCollection(serviceProvider, stoppingToken, configureDelegate);
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

            if (!IsCancellationRequested)
                ServiceTaskResolvers.Add(ctx => { appTaskDelegate(ctx); });

            return this;
        }

        public IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : IAppTask
        {
            //_logger.LogInformation($"{this} - UseAppTask<>.");

            void resolver(IAppContext ctx)
            {
                if (IsCancellationRequested)
                    return;

                var serticeTask = _serviceScope.ServiceProvider.GetRequiredService<TAppTask>();
                serticeTask.Execute(ctx);
            }

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            void resolver(IAppContext ctx)
            {
                if (IsCancellationRequested && !predicate(ctx))
                    return;

                using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, _stoppingToken, branchFlow, createNestedScope))
                {
                    AppTaskCollectionEngine.Run(serviceTaskCollection, ctx);
                }
            }

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            void resolver(IAppContext ctx)
            {
                while (!IsCancellationRequested && predicate(ctx))
                {
                    using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, _stoppingToken, branchFlow, createNestedScope))
                    {
                        AppTaskCollectionEngine.Run(serviceTaskCollection, ctx);
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
