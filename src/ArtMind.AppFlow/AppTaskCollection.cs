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

        #region IAppTaskCollection - UseAppTask
        
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

        public IAppTaskCollection<TResult> UseAppTask<TFlowTaskStart, TResult>() where TFlowTaskStart : IAppTask<IAppContext, TResult>
        {
            var appTaskCollection = new AppTaskCollection<TResult>(this);

            var resolver = new Func<Action<IAppContext>>(() =>
            {
                return (ctx) =>
                {
                    if (IsCancellationRequested)
                        return;

                    var serviceTask = _serviceScope.ServiceProvider.GetRequiredService<TFlowTaskStart>();
                    var rezult = serviceTask.Execute(ctx);

                    appTaskCollection.Run(rezult);
                };
            });

            UseAppTask(resolver);

            return appTaskCollection;
        }

        #endregion

        #region IAppTaskCollection - UseIfBranch

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, branchFlow.ToConfigurableAction(), null, createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> ifBranchFlow, Action<IAppTaskCollection> elseBranchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, ifBranchFlow.ToConfigurableAction(), elseBranchFlow.ToConfigurableAction(), createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> branchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, branchFlow, null, createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> ifBranchFlow, Action<IConfiguration, IAppTaskCollection> elseBranchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, ifBranchFlow, elseBranchFlow, createNestedScope);
        }

        #endregion

        #region IAppTaskCollection - UseWhileBranch

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

        internal T GetRequiredService<T>()
        {
            return _serviceScope.ServiceProvider.GetRequiredService<T>();
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey} [{ServiceAppTaskResolvers.Count}]";
        }

        #endregion
    }

    internal class AppTaskCollection<T> : IAppTaskCollection<T>, IAsyncDisposable, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly AppTaskCollection _rootTaskCollection;
        private bool _disposed;
        private Action<T> _action;

        public bool IsCancellationRequested => _rootTaskCollection.IsCancellationRequested;        

        #region Ctor

        internal AppTaskCollection(AppTaskCollection rootTaskCollection)
        {
            _rootTaskCollection = rootTaskCollection;
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
            if (_action != null)
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

                _action = null;

                //_logger.LogTrace($"{this} - Disposed.");});
            }

            _disposed = true;
        }

        #endregion

        public void Run(T input)
        {
           _action?.Invoke(input);
        }

        #region IAppTaskCollection<T> - UseAppTask

        public IAppTaskCollection<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : IAppTask<T, TResult>
        {
            var next = new AppTaskCollection<TResult>(_rootTaskCollection);

            _action = new Action<T>((input) =>
            {
                if (IsCancellationRequested)
                    return;

                var serviceTask = _rootTaskCollection.GetRequiredService<TFlowTask>();
                var rezult = serviceTask.Execute(input);

                next.Run(rezult);
            });

            return next;
        }

        public IAppTaskCollection UseAppTask<TFlowTask>() where TFlowTask : IAppTask<T>
        {
            _action = new Action<T>((input) =>
            {
                if (IsCancellationRequested)
                    return;

                var serviceTask = _rootTaskCollection.GetRequiredService<TFlowTask>();
                serviceTask.Execute(input);
                
                return;
            });

            return _rootTaskCollection;
        }

        #endregion
    }
}
