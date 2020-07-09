﻿using ArtMind.AppFlow.Abstractions;
using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtMind.AppFlow.UseCase.Service.Workers
{
    public class FinishWorker : BaseWorker, IAppTask
    {
        private readonly ILogger<FinishWorker> _logger;
        private readonly ISingletonDependency _singletonDependency;
        private readonly IScopedDependency _scopedDependency;
        private readonly ITransientDependency _transientDependency;

        public FinishWorker(ILogger<FinishWorker> logger,
            ISingletonDependency singletonDependency,
            IScopedDependency scopedDependency,
            ITransientDependency transientDependency)
        {
            _logger = logger;
            _singletonDependency = singletonDependency;
            _scopedDependency = scopedDependency;
            _transientDependency = transientDependency;
        }

        public void Execute(IAppContext context)
        {
            _logger.LogInformation(this, context, _singletonDependency, _scopedDependency, _transientDependency);
            _logger.LogInformation("App run successfuly, clossing the app.");
            Task.Delay(1000);
        }
    }
}