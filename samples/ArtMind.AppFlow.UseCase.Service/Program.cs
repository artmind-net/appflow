using System;
using ArtMind.AppFlow.UseCase.Service.Abstractions;
using ArtMind.AppFlow.UseCase.Service.Dependencies;
using ArtMind.AppFlow.UseCase.Service.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                AppSettings appSettings = hostContext.Configuration
                .GetSection("AppSettings")
                .Get<AppSettings>();

                services.AddSingleton(appSettings);

                services.AddSingleton<ISingletonDependency, SingletonDependency>();
                services.AddScoped<IScopedDependency, ScopedDependency>();
                services.AddTransient<ITransientDependency, TransientDependency>();

                services.AddTransient<InitCounterWorker>();
                services.AddTransient<IfWorker>();
                services.AddTransient<WhileWorker>();
                services.AddTransient<ErrorWorker>();
                services.AddTransient<FinishWorker>();
            })
            .ConfigureLogging(logBuilder => 
            {
                logBuilder.AddConsole();
                logBuilder.AddDebug();
            })
            //.RegisterAppFlow(AppOptions.Postpone(), flow => // your application will behave as a console app
            .RegisterServiceFlow(ServiceOptions.Options(3,TimeSpan.FromSeconds(3), DateTime.UtcNow.AddSeconds(2)) , (cfg, flow) => // // your application will behave as an OS Service
            {
                AppSettings appSettings = cfg
                    .GetSection("AppSettings")
                    .Get<AppSettings>();

                flow
                .UseAppTask<InitCounterWorker>()
                .UseIfBranch(ctx => ctx.HasCounter(), branchFlow =>
                {
                    branchFlow
                    .UseAppTask<IfWorker>()
                    .UseAppTask<IfWorker>()
                    .UseWhileBranch(ctx => ctx.GetCounter() < 5, branchFlow =>
                    {
                        branchFlow.UseAppTask<WhileWorker>();
                    });
                }, elseBranchFlow =>
                {
                    elseBranchFlow
                        .UseAppTask<ErrorWorker>();
                }, true)
                //.UseAppTask<ExWorker>() // uncomment this line to throw an error.
                .UseAppTask<FinishWorker>();
            });
        // use dummy task
    }
}
