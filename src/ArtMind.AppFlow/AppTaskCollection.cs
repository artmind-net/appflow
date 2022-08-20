using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArtMind.AppFlow
{
    internal class AppTaskCollection : IAppTaskCollection, IAsyncDisposable, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly bool _hasDisposableScope;
        private readonly IServiceCollection _service;
        private readonly IServiceScope _serviceScope;
        private readonly CancellationToken _stoppingToken;
        private readonly ILogger<AppFlowHost> _logger;

        /// <summary>
        /// Overrides dessioncion to use or not the inner scope.
        /// </summary>
        private bool _hasNewServiceCollectionItems;
        private bool _disposed;

        public bool IsCancellationRequested => _stoppingToken.IsCancellationRequested;
        public List<Func<Action<IAppContext>>> ServiceAppTaskResolvers { get; } = new List<Func<Action<IAppContext>>>();

        #region Ctor & Builders & Destroyers

        private AppTaskCollection(
            CancellationToken stoppingToken, 
            Action<IConfiguration,IAppTaskCollection> configureDelegate, 
            IServiceCollection service, 
            IServiceScope serviceScope, 
            bool useInnerScope)
        {
            _stoppingToken = stoppingToken;            
            _service = service;            
            _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<AppFlowHost>>();
            var configuration = serviceScope.ServiceProvider.GetService<IConfiguration>();
            configureDelegate(configuration, this);

            // since the upper scope is missing the registered services, the creation of the inner scope starts to be a must. 
            if (_hasNewServiceCollectionItems)
            {
                if (useInnerScope == false)
                    _logger.LogWarning("Because the global scoped ServiceCollection is missing the ArtMind AppTask types (DependencyInjection), " +
                        "the configuration for using the upper scope will be ignored and an inner scope will be created.");

                _hasDisposableScope = true;
                _serviceScope = _service.BuildServiceProvider().CreateScope();
            }
            else 
            {
                _hasDisposableScope = useInnerScope;
                _serviceScope = serviceScope;
            }
        }

        internal static AppTaskCollection CreateRoot(
            CancellationToken stoppingToken, 
            Action<IConfiguration, IAppTaskCollection> configureDelegate, 
            IServiceCollection services,
            IServiceProvider serviceProvider)
        {
            return new AppTaskCollection(stoppingToken, configureDelegate, services, serviceProvider.CreateScope(), true);
        }

        private AppTaskCollection CreateNested(
            AppTaskCollection parrent,
            Action<IConfiguration, IAppTaskCollection> configureDelegate,
            bool useInnerScope)
        {
            var scope = useInnerScope ? parrent._serviceScope.ServiceProvider.CreateScope() : parrent._serviceScope;

            return new AppTaskCollection(parrent._stoppingToken, configureDelegate, parrent._service, scope, useInnerScope);
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

            if (IsCancellationRequested)
                return this;

           ServiceAppTaskResolvers.Add(appTaskResolver);

            return this;
        }

        public IAppTaskCollection UseAppTask(Action<IAppContext> appTaskAction)
        {
            //_logger.LogTrace($"{this} - UseAppTask.");

            if (IsCancellationRequested)
                return this;

            ServiceAppTaskResolvers.Add(() =>
            {
                if (IsCancellationRequested)
                    return (ctx) => { };

                return appTaskAction;
            });

            return this;
        }

        public IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : class, IAppTask
        {
            //_logger.LogTrace($"{this} - UseAppTask<>.");

            RegisterTypeIfNot<TAppTask>();

            return UseAppTask((ctx) => _serviceScope.ServiceProvider.GetRequiredService<TAppTask>().Execute(ctx));
        }

        public IAppTaskCollection<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : class, IAppTask<IAppContext, TResult>
        {
            var appTaskCollection = new AppTaskCollection<TResult>(this);

            RegisterTypeIfNot<TFlowTask>();

            Action<IAppContext> resolver = (ctx) =>
            {
                if (IsCancellationRequested)
                    return;

                var serviceTask = _serviceScope.ServiceProvider.GetRequiredService<TFlowTask>();
                var rezult = serviceTask.Execute(ctx);

                appTaskCollection.Run(rezult);
            };

            UseAppTask(resolver);

            return appTaskCollection;
        }

        #endregion

        #region IAppTaskCollection - UseIfBranch

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            return UseIfElseBranch(predicate, branchFlow.ToConfigurableAction(), null, createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> ifBranchFlow, Action<IAppTaskCollection> elseBranchFlow, bool createNestedScope = true)
        {
            return UseIfElseBranch(predicate, ifBranchFlow.ToConfigurableAction(), elseBranchFlow.ToConfigurableAction(), createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            return UseIfElseBranch(predicate, branchFlow, null, createNestedScope);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> ifBranchFlow, Action<IConfiguration, IAppTaskCollection> elseBranchFlow, bool createNestedScope = true)
        {
            return UseIfElseBranch(predicate, ifBranchFlow, elseBranchFlow, createNestedScope);
        }

        #endregion

        #region IAppTaskCollection - UseWhileBranch

        public IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            Action<IAppContext> resolver = (ctx) =>
            {
                while (!IsCancellationRequested && predicate(ctx))
                {
                    using (var serviceTaskCollection = CreateNested(this, branchFlow.ToConfigurableAction(), createNestedScope))
                    {
                        serviceTaskCollection.Run(ctx);
                    }
                }
            };

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> branchFlow, bool createNestedScope = true)
        {
            Action<IAppContext> resolver =  (ctx) =>
            {
                while (!IsCancellationRequested && predicate(ctx))
                {
                    using (var serviceTaskCollection = CreateNested(this, branchFlow, createNestedScope))
                    {
                        serviceTaskCollection.Run(ctx);
                    }
                }
            };

            return UseAppTask(resolver);
        }

        #endregion

        #region Helpers

        private IAppTaskCollection UseIfElseBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppTaskCollection> ifBranchFlow, Action<IConfiguration, IAppTaskCollection> elseBranchFlow, bool createNestedScope)
        {
            Action<IAppContext> resolver = (ctx) =>
            {
                if (!IsCancellationRequested && predicate(ctx))
                {
                    using (var serviceTaskCollection = CreateNested(this, ifBranchFlow, createNestedScope))
                    {
                        serviceTaskCollection.Run(ctx);
                    }
                }
                else if (elseBranchFlow != null)
                {
                    using (var serviceTaskCollection = CreateNested(this, elseBranchFlow, createNestedScope))
                    {
                        serviceTaskCollection.Run(ctx);
                    }
                }
            };

            return UseAppTask(resolver);
        }

        internal T GetRequiredService<T>()
        {
            return _serviceScope.ServiceProvider.GetRequiredService<T>();
        }

        internal void RegisterTypeIfNot<T>() where T : class
        {
            if (_service != null && !_service.Any(x => x.ServiceType == typeof(T)))
            {
                _hasNewServiceCollectionItems = true;
                _service.AddTransient<T>();
            }
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

        #region Ctor & Builders & Destroyers

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

        public IAppTaskCollection<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : class, IAppTask<T, TResult>
        {
            var next = new AppTaskCollection<TResult>(_rootTaskCollection);

            _rootTaskCollection.RegisterTypeIfNot<TFlowTask>();

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

        public IAppTaskCollection UseAppTask<TFlowTask>() where TFlowTask : class, IAppTask<T>
        {
            _rootTaskCollection.RegisterTypeIfNot<TFlowTask>();

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
