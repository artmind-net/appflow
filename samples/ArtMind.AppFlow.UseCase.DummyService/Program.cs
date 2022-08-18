using ArtMind.AppFlow;
using ArtMind.AppFlow.UseCase.DummyService.Workers;

await Host
    .CreateDefaultBuilder(args)
    .RegisterAppFlow(flow =>
     {
         flow
         .UseAppTask<WorkerStart, int>()
         .UseAppTask<WorkerMidd, string>()
         .UseAppTask<WorkerEnd>();
     })
    .Build()
    .RunAsync();