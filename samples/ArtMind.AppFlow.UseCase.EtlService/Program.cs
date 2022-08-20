using ArtMind.AppFlow;
using ArtMind.AppFlow.UseCase.EtlService;
using ArtMind.AppFlow.UseCase.EtlService.Core;
using ArtMind.AppFlow.UseCase.EtlService.Workers;

IHost host = Host.CreateDefaultBuilder(args)    
    .RegisterAppFlow((cfg, services) => 
    {
        AppSettings appSettings = cfg
                  .GetSection("AppSettings")
                  .Get<AppSettings>();

        services.AddSingleton(appSettings);

    }, (cfg, flow) =>
    {
        flow
        .UseAppTask<ExtractionWorker, IEnumerable<IMyData>>()
        .UseAppTask<TransformationWorker, IEnumerable<IGrouping<string, IMyData>>>()
        .UseAppTask<LoadWorker>();
    })
    .Build();

await host.RunAsync();
