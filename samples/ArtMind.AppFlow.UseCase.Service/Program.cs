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
            .ConfigureLogging(logBuilder =>
            {
                logBuilder.AddConsole();
                logBuilder.AddDebug();
            })
            //.RegisterAppFlow(AppOptions.Postpone(), flow => // your application will behave as a console app
            .RegisterServiceFlow((cfg) =>
            {
                AppSettings appSettings = cfg
                    .GetSection("AppSettings")
                    .Get<AppSettings>();

                return ServiceOptions.Options(4, TimeSpan.FromSeconds(3), DateTime.UtcNow.AddSeconds(2));
            }, (cfg, services) =>
            {
                AppSettings appSettings = cfg
                   .GetSection("AppSettings")
                   .Get<AppSettings>();

                services.AddSingleton(appSettings);
                services.AddSingleton<ISingletonDependency, SingletonDependency>();
                services.AddScoped<IScopedDependency, ScopedDependency>();
                services.AddTransient<ITransientDependency, TransientDependency>();

                services.AddTransient<InitCounterWorker>();
                services.AddTransient<IfWorker>();
                services.AddTransient<WhileWorker>();
                services.AddTransient<TraceWorkerWithError>();
                services.AddTransient<TraceWorkerClosure>();

            }, (cfg, flow) => // // your application will behave as an OS Service
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
                        .UseAppTask<TraceWorkerWithError>();
                }, true)
                .UseAppTask<TraceWorkerWithError>() // uncomment this line to throw an error.
                .UseAppTask<TraceWorkerClosure>();
            });
        // use dummy task
    }
}
