using ArtMind.AppFlow;
using ArtMind.AppFlow.UseCase;

await Host
    .CreateDefaultBuilder(args)
    .RegisterServiceFlow(flow =>
     {
         flow
         .UseAppTask<TypedStartWorker, int>()
         .UseAppTask<TypedMiddWorker, string>()
         .UseAppTask<TypedEndWorker>();
     })
    .Build()
    .RunAsync();