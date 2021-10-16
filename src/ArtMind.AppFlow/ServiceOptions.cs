using System;

namespace ArtMind.AppFlow
{
    public class ServiceOptions
    {
        private readonly ulong _occurrenceLimit;
        private readonly TimeSpan _recurrenceInterval;
        private readonly DateTime? _scheduleAt;

        private ServiceOptions(ulong occurrenceLimit, TimeSpan recurrenceInterval, DateTime? scheduleAt = null)
        {
            _occurrenceLimit = occurrenceLimit;
            _recurrenceInterval = recurrenceInterval;
            _scheduleAt = scheduleAt;
        }

        internal static  ServiceOptions Default => new ServiceOptions(0, TimeSpan.Zero);

        /// <summary>
        /// Setting service occurrence limit.
        /// </summary>
        /// <param name="occurrenceLimit">The number of service flow cycles to be executed</param>
        /// <returns></returns>
        public static ServiceOptions Occurrence(ulong occurrenceLimit) => new ServiceOptions(occurrenceLimit, TimeSpan.Zero);

        /// <summary>
        /// Setting service cycle recurrence interval.
        /// </summary>
        /// <param name="recurrenceInterval">The minimum time interval duration of a service flow cycle, until to start next session</param>
        /// <returns></returns>
        public static ServiceOptions Recurrence(TimeSpan recurrenceInterval) => new ServiceOptions(0, recurrenceInterval);

        /// <summary>
        /// Schedule service start.
        /// </summary>
        /// <param name="scheduleAt">UTC time when to schedule service to start</param>
        /// <returns></returns>
        public static ServiceOptions Schedule(DateTime scheduleAt) => new ServiceOptions(0, TimeSpan.Zero, scheduleAt);

        /// <summary>
        /// Setting service occurrence limit and cycle recurrence interval.
        /// </summary>
        /// <param name="occurrenceLimit">The number of service flow cycles to be executed</param>
        /// <param name="recurrenceInterval">The minimum time interval duration of a service flow cycle, until to start next session</param>
        /// <param name="scheduleAt">UTC time when to schedule service to start</param>
        /// <returns></returns>
        public static ServiceOptions Options(ulong occurrenceLimit, TimeSpan recurrenceInterval, DateTime scheduleAt) => new ServiceOptions(occurrenceLimit, recurrenceInterval, scheduleAt);

        #region Internal helpers

        internal bool IsCycleLimitExceeded(ulong cycleCounter)
        {
            return _occurrenceLimit != 0 && _occurrenceLimit <= cycleCounter;
        }

        internal bool ShouldDelay(TimeSpan duration, out TimeSpan delay)
        {
            delay = TimeSpan.Zero;

            if (duration != TimeSpan.Zero && _recurrenceInterval > duration)
                delay = _recurrenceInterval - duration;

            return delay != TimeSpan.Zero;
        }

        internal bool ShouldPostpone(out TimeSpan postpone)
        {
            postpone = TimeSpan.Zero;

            if (_scheduleAt.HasValue && _scheduleAt.Value > DateTime.UtcNow)
            {
                postpone = _scheduleAt.Value - DateTime.UtcNow;
            }

            return postpone != TimeSpan.Zero;
        }

        #endregion
    }
}
