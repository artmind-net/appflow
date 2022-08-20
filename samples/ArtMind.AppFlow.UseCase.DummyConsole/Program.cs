using ArtMind.AppFlow;
using ArtMind.AppFlow.Abstractions;
using Microsoft.Extensions.Hosting;
using System;

Host
    .CreateDefaultBuilder()
    .RegisterAppFlow((cfg, flow) =>
    {
        flow
        .UseAppTask(
            (appContext) =>
            Console.WriteLine("Hellow")
        )
        .UseAppTask((c) => Console.WriteLine("Hellow 2"));
    })
    .Build()
    .Run();

