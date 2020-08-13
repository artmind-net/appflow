using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow
{
    //public delegate void AppTaskResolver(IAppContext ctx);

    public class AppTaskCollection : IAppTaskCollection, IAsyncDisposable, IDisposable
    {
        private readonly string _instanceKey = Guid.NewGuid().ToString("N");
        private readonly bool _hasDisposableScope;
        private readonly IServiceScope _serviceScope;
        private readonly CancellationToken _stoppingToken;
        //private readonly ILogger<AppFlowHost> _logger;
        private bool _disposed = false;
        
        public bool IsCancellationRequested { get => _stoppingToken.IsCancellationRequested; }
        public List<Func<Action<IAppContext>>> ServiceAppTaskResolvers { get; } = new List<Func<Action<IAppContext>>>();

        #region Ctor

        private AppTaskCollection(IServiceScope serviceScope, CancellationToken stoppingToken, Action<IAppTaskCollection> configureDelegate, bool useInnerScope)
        {
            _hasDisposableScope = useInnerScope;
            _serviceScope = useInnerScope ? serviceScope.ServiceProvider.CreateScope() : serviceScope;
            _stoppingToken = stoppingToken;
            //_logger = _serviceScope.ServiceProvider.GetRequiredService<ILogger<AppFlowHost>>();

            configureDelegate(this);
        }

        public AppTaskCollection(IServiceProvider serviceProvider, CancellationToken stoppingToken, Action<IAppTaskCollection> configureDelegate)
            : this(serviceProvider.CreateScope(), stoppingToken, configureDelegate, false)
        {
            this._hasDisposableScope = true;
        }

        public static AppTaskCollection CreateRoot(IServiceProvider serviceProvider, CancellationToken stoppingToken, Action<IAppTaskCollection> configureDelegate)
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
                //_logger.LogInformation($"{this} - Disposing ...");

                ServiceAppTaskResolvers.Clear();

                if (_hasDisposableScope)
                    _serviceScope.Dispose();

                //_logger.LogInformation($"{this} - Disposed.");});
            }

            _disposed = true;
        }

        #endregion

        #region IAppTaskCollection

        public IAppTaskCollection UseAppTask(Func<Action<IAppContext>> appTaskResolver)
        {
            //_logger.LogInformation($"{this} - UseAppTask.");

            if (!IsCancellationRequested)
                ServiceAppTaskResolvers.Add(appTaskResolver);

            return this;
        }

        public IAppTaskCollection UseAppTask<TAppTask>() where TAppTask : IAppTask
        {
            //_logger.LogInformation($"{this} - UseAppTask<>.");

            var resolver = new Func<Action<IAppContext>>(() => 
            {
                if (IsCancellationRequested)
                    return (ctx) => { };

                var serticeTask = _serviceScope.ServiceProvider.GetRequiredService<TAppTask>();
                return serticeTask.Execute;
            });

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseIfBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = false)
        {
            var resolver = new Func<Action<IAppContext>>(() =>
            {
                if (IsCancellationRequested)
                    return (ctx) => { };

                return (ctx) =>
                {
                    using (var serviceTaskCollection = new AppTaskCollection(_serviceScope, _stoppingToken, branchFlow, createNestedScope))
                    {
                        serviceTaskCollection.Run(ctx);
                    }
                };
            });

            return UseAppTask(resolver);
        }

        public IAppTaskCollection UseWhileBranch(Predicate<IAppContext> predicate, Action<IAppTaskCollection> branchFlow, bool createNestedScope = false)
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

        public override string ToString()
        {
            return $"{this.GetType().Name}: {_instanceKey} [{ServiceAppTaskResolvers.Count}]";
        }
    }
}
