using System;

namespace ArtMind.AppFlow
{
    public interface IServiceOptions
    {
        /// <summary>
        /// Maximum number of service flow cycles to be executed.
        /// Default = 0 => unlimited.
        /// </summary>
        ulong CyclesLimit { get; }

        /// <summary>
        /// Minimum time interval duration of a service flow cycle, until to start next session.
        /// Default = TimeSpan.Zero
        /// </summary>
        TimeSpan MinCycleDuration { get; }
    }

    public class ServiceOptions : IServiceOptions
    {
        public ServiceOptions(ulong maxCycles) : this(maxCycles, TimeSpan.Zero) { }

        public ServiceOptions(TimeSpan minCycleDuration) : this(0, minCycleDuration) { }

        public ServiceOptions(ulong maxCycles, TimeSpan minCycleDuration)
        {
            CyclesLimit = maxCycles;
            MinCycleDuration = minCycleDuration;
        }

        public static ServiceOptions Default=> new ServiceOptions(0, TimeSpan.Zero);

        public ulong CyclesLimit { get; }
        public TimeSpan MinCycleDuration { get; }
    }
}
