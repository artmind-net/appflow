# AppFlow registration 
The simplest way to create .NET app flow is using the 'IHostBuilder' extension method 'ArtMind.AppFlow.RegisterAppFlow.

# todo
1. to do add task name
1. create uniform logger
1. add async tasks


```csharp
using ArtMind.AppFlow.Abstractions;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddTransient<InitCounterWorker>();
            services.AddTransient<IfWorker>();
            services.AddTransient<WhileWorker>();
            services.AddTransient<FinishWorker>();
        })
        .RegisterServiceFlow(flow =>
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
```