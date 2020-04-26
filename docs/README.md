The simplest way to create .NET app flow is using the 'IHostBuilder' extension method 'ArtMind.AppFlow.RegisterAppFlow.

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
         return Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddTransient<StartWorker>();
                        services.AddTransient<WaitWorker>();
                        services.AddTransient<StuffWorker>();
                        services.AddTransient<FinishWorker>();
                    })
                    .RegisterAppFlow(rootFlow =>
                    {
                        rootFlow.UseAppTask<StartWorker>()
                                .UseAppTask<WaitWorker>()
                                .UseIfBranch(ctx => ctx.HasStuff(), branchFlow =>
                                {
                                    branchFlow.UseAppTask<StuffWorker>()
                                              .UseAppTask<WaitWorker>();
                                })
                                .UseAppTask<FinishWorker>();
                    });
    }
}
```