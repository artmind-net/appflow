using ArtMind.AppFlow;
using ArtMind.AppFlow.UseCase;
using Microsoft.Extensions.Hosting;

Host
    .CreateDefaultBuilder()
    .RegisterConsoleFlow((cfg, flow) =>
    {
        flow
        .UseAppTask<HelloWorldWorker>()
        .UseAppTask((c) => Console.WriteLine("I'm a console app inline task."));
    })
    .Build()
    .Run();

