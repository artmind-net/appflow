using ArtMind.AppFlow.UseCase.Service.Abstractions;
using ArtMind.AppFlow.UseCase.Service.Dependencies;
using ArtMind.AppFlow.UseCase.Service.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArtMind.AppFlow.UseCase.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                AppSettings appSetings = hostContext.Configuration
                .GetSection("AppSettings")
                .Get<AppSettings>();

                services.AddSingleton(appSetings);

                services.AddSingleton<ISingletonDependency, SingletonDependency>();
                services.AddScoped<IScopedDependency, ScopedDependency>();
                services.AddTransient<ITransientDependency, TransientDependency>();

                services.AddTransient<InitCounterWorker>();
                services.AddTransient<IfWorker>();
                services.AddTransient<WhileWorker>();
                services.AddTransient<FinishWorker>();
            })
            .RegisterAppFlow(flow =>
            {
                flow.UseAppTask<InitCounterWorker>()
                .UseIfBranch(ctx => ctx.HasCounter(), branchFlow =>
                {
                    branchFlow
                    .UseAppTask<IfWorker>()
                    .UseAppTask<IfWorker>()
                    .UseWhileBranch(ctx => ctx.GetCounter() < 5, branchFlow =>
                    {
                        branchFlow.UseAppTask<WhileWorker>();
                    });
                }, false)
                .UseAppTask<FinishWorker>();
            });
    }
}