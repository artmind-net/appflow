﻿using ArtMind.AppFlow.UseCase.Service.Abstractions;
using Microsoft.Extensions.Logging;
using System;

namespace ArtMind.AppFlow.UseCase.Service
{
    public static class AppExtensions
    {
        public static bool HasCounter(this IAppContext ctx)
        {
            return ctx.HasKey(AppSettings.CounterKey);
        }

        public static int GetCounter(this IAppContext ctx)
        {
            //return ctx.Get<int>(AppSettings.CounterKey);

            // debug
            var x = ctx.Get<int>(AppSettings.CounterKey); ;
            return x;
        }

        public static void LogInformation(this ILogger logger, IAppTask self, IAppContext ctx, ISingletonDependency sng, IScopedDependency scp, ITransientDependency trn)
        {
            logger.LogInformation($"{self} - executing ...{Environment.NewLine} context: {ctx} | dependencies: [{sng}, {scp}, {trn}]");
        }
        public static void LogDebug(this ILogger logger, IAppTask self, IAppContext ctx, ISingletonDependency sng, IScopedDependency scp, ITransientDependency trn)
        {
            logger.LogDebug($"{self} - executing ...{Environment.NewLine} context: {ctx} | dependencies: [{sng}, {scp}, {trn}]");
        }
    }
}
