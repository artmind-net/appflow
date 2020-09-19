# ![Logo](docs/icons/logo_80x80.png) ArtMind
## AppFlow - Orchestration framework for .NET applications
The simple & quick way to create a .NET app flow.
A management library for .NET applications with  a fluent API and easy to set up.

[![Build Status](https://dev.azure.com/artmind-net/AppFlow/_apis/build/status/live_build?branchName=master)](https://dev.azure.com/artmind-net/AppFlow/_build/latest?definitionId=3&branchName=master)

[![latest version] (https://img.shields.io/nuget/v/ArtMind.AppFlow)](https://www.nuget.org/packages/ArtMind.AppFlow) 
[![downloads]      (https://img.shields.io/nuget/dt/ArtMind.AppFlow)](https://www.nuget.org/packages/ArtMind.AppFlow/)


## Find out more
- [Homepage](https://www.artmind.ro)
- [NuGet Package](https://www.nuget.org/packages/ArtMind.AppFlow)


## Use RegisterAppFlow() and your application will behave as a console app
```csharp
.RegisterAppFlow(flow => 
{
    flow
        .UseAppTask<InitCounterWorker>()
        .UseIfBranch(ctx => ctx.HasCounter(), branchFlow =>
        {
            branchFlow
            .UseAppTask<IfWorker>()
            .UseWhileBranch(ctx => ctx.GetCounter() < 5, branchFlow =>
            {
                branchFlow.UseAppTask<WhileWorker>();
            });
        }, true)
        .UseAppTask<FinishWorker>();
});
```

## Use RegisterServiceFlow() and your application will behave as an OS Service
```csharp
.RegisterServiceFlow(flow =>
{
    flow
        .UseAppTask<InitCounterWorker>()
        .UseIfBranch(ctx => ctx.HasCounter(), branchFlow =>
        {
            branchFlow
            .UseAppTask<IfWorker>()
            .UseWhileBranch(ctx => ctx.GetCounter() < 5, branchFlow =>
            {
                branchFlow.UseAppTask<WhileWorker>();
            });
        }, true)
        .UseAppTask<FinishWorker>();
});
```


## Program.cs
```csharp
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
                services.AddTransient<AppWorker1>();
                services.AddTransient<AppWorker2>();
                services.AddTransient<AppWorker3>();
            })
            .ConfigureLogging(logBuilder => 
            {
                logBuilder.AddConsole();
                logBuilder.AddDebug();
            })
            .RegisterAppFlow(flow => // your application will behave as a console app
            //.RegisterServiceFlow(flow => // // your application will behave as an OS Service
            {
                //your flow here
                flow
                    .UseAppTask<AppWorker1>()
                    .UseAppTask<AppWorker2>()
                    .UseAppTask<AppWorker3>();
            });
    }
```