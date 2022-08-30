using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    internal class AppFlowBuilder : IAppFlowBuilder, IAsyncDisposable, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly bool _hasDisposableScope;
        private readonly IServiceScope _serviceScope;
        private readonly CancellationToken _stoppingToken;
        private readonly ILogger<ConsoleAppHost> _logger;

        /// <summary>
        /// Overrides decision to use or not the inner scope.
        /// </summary>
        private bool _disposed;

        public bool IsCancellationRequested => _stoppingToken.IsCancellationRequested;
        public List<Func<Action<IAppContext>>> ServiceAppTaskResolvers { get; } = new List<Func<Action<IAppContext>>>();

        #region Ctor & Builders & Destroyers

        private AppFlowBuilder(
            CancellationToken stoppingToken, 
            Action<IConfiguration,IAppFlowBuilder> configureDelegate,
            IServiceScope serviceScope, 
            bool useInnerScope)
        {
            _stoppingToken = stoppingToken;
            _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<ConsoleAppHost>>();
            var configuration = serviceScope.ServiceProvider.GetService<IConfiguration>();
            configureDelegate(configuration, this);

            _hasDisposableScope = useInnerScope;
            _serviceScope = serviceScope;
        }

        internal static AppFlowBuilder CreateRoot(
            CancellationToken stoppingToken, 
            Action<IConfiguration, IAppFlowBuilder> configureDelegate,
            IServiceProvider serviceProvider)
        {
            return new AppFlowBuilder(stoppingToken, configureDelegate, serviceProvider.CreateScope(), true);
        }

        private AppFlowBuilder CreateNested(
            AppFlowBuilder parrent,
            Action<IConfiguration, IAppFlowBuilder> configureDelegate,
            bool useInnerScope)
        {
            var scope = useInnerScope ? parrent._serviceScope.ServiceProvider.CreateScope() : parrent._serviceScope;

            return new AppFlowBuilder(parrent._stoppingToken, configureDelegate, scope, useInnerScope);
        }

        public static IEnumerable<Type> GetWorkerTypes(Action<IConfiguration, IAppFlowBuilder> configureDelegate, IConfiguration configuration)
        {
            var root = new AppFlowBuilderAbstract(configureDelegate, configuration);

            return root.WorkerTypes;
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

        public IAppFlowBuilder UseAppTask(Func<Action<IAppContext>> appTaskResolver)
        {
            //_logger.LogTrace($"{this} - UseAppTask.");

            if (IsCancellationRequested)
                return this;

           ServiceAppTaskResolvers.Add(appTaskResolver);

            return this;
        }

        public IAppFlowBuilder UseAppTask(Action<IAppContext> appTaskAction)
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

        public IAppFlowBuilder UseAppTask<TAppTask>() where TAppTask : class, IAppTask
        {
            //_logger.LogTrace($"{this} - UseAppTask<>.");

            return UseAppTask((ctx) => _serviceScope.ServiceProvider.GetRequiredService<TAppTask>().Execute(ctx));
        }

        public IAppFlowBuilder<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : class, IAppTask<IAppContext, TResult>
        {
            var appTaskCollection = new AppFlowBuilder<TResult>(this);

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

        public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> branchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, branchFlow.ToConfigurableAction(), null, createNestedScope);
        }

        public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> ifBranchFlow, Action<IAppFlowBuilder> elseBranchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, ifBranchFlow.ToConfigurableAction(), elseBranchFlow.ToConfigurableAction(), createNestedScope);
        }

        public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> branchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, branchFlow, null, createNestedScope);
        }

        public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> ifBranchFlow, Action<IConfiguration, IAppFlowBuilder> elseBranchFlow, bool createNestedScope = false)
        {
            return UseIfElseBranch(predicate, ifBranchFlow, elseBranchFlow, createNestedScope);
        }

        #endregion

        #region IAppTaskCollection - UseWhileBranch

        public IAppFlowBuilder UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> branchFlow, bool createNestedScope = false)
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

        public IAppFlowBuilder UseWhileBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> branchFlow, bool createNestedScope = false)
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

        private IAppFlowBuilder UseIfElseBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> ifBranchFlow, Action<IConfiguration, IAppFlowBuilder> elseBranchFlow, bool createNestedScope)
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
                else if (!IsCancellationRequested && elseBranchFlow != null)
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

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey} [{ServiceAppTaskResolvers.Count}]";
        }

        #endregion

        #region Internal 

        private class AppFlowBuilderAbstract : IAppFlowBuilder
        {
            private readonly IConfiguration _configuration;
            internal List<Type> WorkerTypes { get; private set; } = new List<Type>();

            internal AppFlowBuilderAbstract(Action<IConfiguration, IAppFlowBuilder> configureDelegate, IConfiguration configuration)
            {
                _configuration = configuration;

                configureDelegate(configuration, this);
            }

            public IAppFlowBuilder UseAppTask(Func<Action<IAppContext>> appTaskResolver)
            {
                return this;
            }

            public IAppFlowBuilder UseAppTask(Action<IAppContext> appTaskAction)
            {
                return this;
            }

            public IAppFlowBuilder UseAppTask<TAppTask>() where TAppTask : class, IAppTask
            {
                WorkerTypes.Add(typeof(TAppTask));

                return this;
            }

            public IAppFlowBuilder<TOut> UseAppTask<TFlowTask, TOut>() where TFlowTask : class, IAppTask<IAppContext, TOut>
            {
                WorkerTypes.Add(typeof(TFlowTask));

                return new AppFlowAbstractBuilder<TOut>(this);
            }

            #region If Branch

            public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> branchFlow, bool createNestedScope = false)
            {
                return UseIfElseBranch(predicate, branchFlow.ToConfigurableAction(), null, createNestedScope);
            }

            public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> ifBranchFlow, Action<IAppFlowBuilder> elseBranchFlow, bool createNestedScope = false)
            {
                return UseIfElseBranch(predicate, ifBranchFlow.ToConfigurableAction(), elseBranchFlow.ToConfigurableAction(), createNestedScope);
            }

            public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> branchFlow, bool createNestedScope = false)
            {
                return UseIfElseBranch(predicate, branchFlow, null, createNestedScope);
            }

            public IAppFlowBuilder UseIfBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> ifBranchFlow, Action<IConfiguration, IAppFlowBuilder> elseBranchFlow, bool createNestedScope = false)
            {
                return UseIfElseBranch(predicate, ifBranchFlow, elseBranchFlow, createNestedScope);
            }

            private IAppFlowBuilder UseIfElseBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> ifBranchFlow, Action<IConfiguration, IAppFlowBuilder> elseBranchFlow, bool createNestedScope)
            {
                var ifBranch = new AppFlowBuilderAbstract(ifBranchFlow, this._configuration);
                WorkerTypes.AddRange(ifBranch.WorkerTypes);

                if (elseBranchFlow != null)
                {
                    var elseBranch = new AppFlowBuilderAbstract(elseBranchFlow, this._configuration);
                    WorkerTypes.AddRange(elseBranch.WorkerTypes);
                }

                return this;
            }

            #endregion

            #region While region

            public IAppFlowBuilder UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppFlowBuilder> branchFlow, bool createNestedScope = false)
            {
                return UseWhileBranch(predicate, branchFlow.ToConfigurableAction(), createNestedScope);
            }

            public IAppFlowBuilder UseWhileBranch(Predicate<IAppContext> predicate, Action<IConfiguration, IAppFlowBuilder> branchFlow, bool createNestedScope = false)
            {
                var whileBranch = new AppFlowBuilderAbstract(branchFlow, this._configuration);

                WorkerTypes.AddRange(whileBranch.WorkerTypes);

                return this;
            }

            #endregion
        }

        private class AppFlowAbstractBuilder<T> : IAppFlowBuilder<T>
        {
            private readonly AppFlowBuilderAbstract _inner;

            internal AppFlowAbstractBuilder(AppFlowBuilderAbstract inner)
            {
                _inner = inner;
            }

            public IAppFlowBuilder<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : class, IAppTask<T, TResult>
            {
                _inner.WorkerTypes.Add(typeof(TFlowTask));

                return new AppFlowAbstractBuilder<TResult>(_inner);
            }

            public IAppFlowBuilder UseAppTask<TFlowTask>() where TFlowTask : class, IAppTask<T>
            {
                _inner.WorkerTypes.Add(typeof(TFlowTask));

                return _inner;
            }
        }

        #endregion
    }

    internal class AppFlowBuilder<T> : IAppFlowBuilder<T>, IAsyncDisposable, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly AppFlowBuilder _rootTaskCollection;
        private bool _disposed;
        private Action<T> _action;

        public bool IsCancellationRequested => _rootTaskCollection.IsCancellationRequested;

        #region Ctor & Builders & Destroyers

        internal AppFlowBuilder(AppFlowBuilder rootTaskCollection)
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

        public IAppFlowBuilder<TResult> UseAppTask<TFlowTask, TResult>() where TFlowTask : class, IAppTask<T, TResult>
        {
            var next = new AppFlowBuilder<TResult>(_rootTaskCollection);

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

        public IAppFlowBuilder UseAppTask<TFlowTask>() where TFlowTask : class, IAppTask<T>
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
