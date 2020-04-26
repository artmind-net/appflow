# AppFlow registration 
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
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =&gt;
            {
                services.AddTransient&lt;InitCounterWorker&gt;();
                services.AddTransient&lt;IfWorker&gt;();
                services.AddTransient&lt;WhileWorker&gt;();
                services.AddTransient&lt;FinishWorker&gt;();
            })
            .RegisterServiceFlow(flow =&gt;
            {
                flow.UseAppTask&lt;InitCounterWorker&gt;()
                .UseIfBranch(ctx =&gt; ctx.HasCounter(), branchFlow =&gt;
                {
                    branchFlow
                    .UseAppTask&lt;IfWorker&gt;()
                    .UseAppTask&lt;IfWorker&gt;()
                    .UseWhileBranch(ctx =&gt; ctx.GetCounter() &lt; 5, branchFlow =&gt;
                    {
                        branchFlow.UseAppTask&lt;WhileWorker&gt;();
                    });
                }, false)
                .UseAppTask&lt;FinishWorker&gt;();
            });
    }
}
```