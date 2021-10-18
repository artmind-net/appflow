using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ArtMind.AppFlow
{
    //public delegate void AppTaskResolver(IAppContext ctx);

    internal class AppTaskCollection : IAppTaskCollection, IAsyncDisposable, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly bool _hasDisposableScope;
        private readonly IServiceScope _serviceScope;
        private readonly CancellationToken _stoppingToken;
        //private readonly ILogger<AppFlowHost> _logger;
        private bool _disposed;

        public bool IsCancellationRequested => _stoppingToken.IsCancellationRequested;
        public List<Func<Action<IAppContext>>> ServiceAppTaskResolvers { get; } = new List<Func<Action<IAppContext>>>();

        #region Ctor

        private AppTaskCollection(IServiceScope serviceScope, CancellationToken stoppingToken, Action<IConfiguration, IAppTaskCollection> configureDelegate, bool useInnerScope)
        {
            _hasDisposableScope = useInnerScope;
            _serviceScope = useInnerScope ? serviceScope.ServiceProvider.CreateScope() : serviceScope;
            _stoppingToken = stoppingToken;
            //_logger = _serviceScope.ServiceProvider.GetRequiredService<ILogger<AppFlowHost>>();
            var configuration = _serviceScope.ServiceProvider.GetService<IConfiguration>();
            configureDelegate(configuration, this);
        }

        public AppTaskCollection(IServiceProvider serviceProvider, CancellationToken stoppingToken, Action<IConfiguration, IAppTaskCollection> configureDelegate)
            : this(serviceProvider.CreateScope(), stoppingToken, configureDelegate, false)
        {
            this._hasDisposableScope = true;
        }

        public static AppTaskCollection CreateRoot(IServiceProvider serviceProvider, CancellationToken stoppingToken, Action<IConfiguration, IAppTaskCollection> configureDelegate)
        {
            return new AppTaskCollection(serviceProvider, stoppingToken, configureDelegate);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            // Cascade async dispose calls
            if (ServiceAppTaskResolvers.Any())
            {
                await Task.Run(() => Dispose(true));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)            
                return;

            if (disposing)
            {
                //_logger.LogTrace($"{this} - Disposing ...");

                ServiceAppTaskResolvers.Clear();

                if (_hasDisposableScope)
                    _serviceScope.Dispose();

                //_logger.LogTrace($"{this} - Disposed.");});
            }

            _disposed = true;
        }

        #endregion

        #region IAppTaskCollection

        public IAppTaskCollection UseAppTask(Func<Action<IAppContext>> appTaskResolver)
        {
            //_logger.LogTrace($"{this} - UseAppTask.");

            if (!IsCancellationRequested)
                ServiceAppTaskResolvers.Add(appTaskResolver);

            return this;
        }

        public IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : IAppTask
        {
            //_logger.LogTrace($"{this} - UseAppTask<>.");

            var resolver = new Func<Action<IAppContext>>(() => 
            {
                if (IsCancellationRequested)
                    return (ctx) => { };

                var serviceTask = _serviceScope.ServiceProvider.GetRequiredService<TAppTask>();
                return serviceTask.Execute;
            });

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, branchFlow.ToConfigurableAction(), null, createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> ifBranchFlow, Action<IAppTaskCollection> elseBranchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, ifBranchFlow.ToConfigurableAction(), elseBranchFlow.ToConfigurableAction(), createNestedScope);
        }

        public IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = false)
        {
            var resolver = new Func<Action<IAppContext>>(() =>
            {
                return (ctx) =>
                {
                    while (!IsCancellationRequested && predicate(ctx))
                    {
                        using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, _stoppingToken, branchFlow.ToConfigurableAction(), createNestedScope))
                        {
                            serviceTaskCollection.Run(ctx);
                        }
                    }
                };
            });

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> branchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, branchFlow, null, createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> ifBranchFlow, Action<IConfiguration, IAppTaskCollection> elseBranchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, ifBranchFlow, elseBranchFlow, createNestedScope);
        }

        public IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> branchFlow, bool createNestedScope = false)
        {
            var resolver = new Func<Action<IAppContext>>(() =>
            {
                return (ctx) =>
                {
                    while (!IsCancellationRequested && predicate(ctx))
                    {
                        using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, _stoppingToken, branchFlow, createNestedScope))
                        {
                            serviceTaskCollection.Run(ctx);
                        }
                    }
                };
            });

            return UseAppTask(resolver);
        }

        #endregion

        #region Helpers

        private IAppTaskCollection UseIfElseBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> ifBranchFlow, Action<IConfiguration, IAppTaskCollection> elseBranchFlow, bool createNestedScope)
        {
            var resolver = new Func<Action<IAppContext>>(() =>
            {
                return (ctx) =>
                {
                    if (!IsCancellationRequested && predicate(ctx))
                    {
                        using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, _stoppingToken, ifBranchFlow, createNestedScope))
                        {
                            serviceTaskCollection.Run(ctx);
                        }
                    }
                    else if (elseBranchFlow != null)
                    {
                        using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, _stoppingToken, elseBranchFlow, createNestedScope))
                        {
                            serviceTaskCollection.Run(ctx);
                        }
                    }
                };
            });

            return UseAppTask(resolver);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey} [{ServiceAppTaskResolvers.Count}]";
        }

        #endregion
    }
}
