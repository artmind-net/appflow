using System;

namespace ArtMind.AppFlow
{
    public class ConsoleAppOptions
    {
        private readonly TimeSpan _postponeInterval;
        private readonly DateTime? _scheduleAt;

        private ConsoleAppOptions(TimeSpan postpone, DateTime? schedule)
        {
            _postponeInterval = postpone;
            _scheduleAt = schedule;
        }

        internal static ConsoleAppOptions Default => new ConsoleAppOptions(TimeSpan.Zero, null);

        /// <summary>
        /// Postpone application start.
        /// </summary>
        /// <param name="postponeInterval">Flow will start after the postpone interval elapses, Default = TimeSpan.Zero, means no delay.</param>
        /// <returns></returns>
        public static ConsoleAppOptions Postpone(TimeSpan postponeInterval) => new ConsoleAppOptions(postponeInterval, null);

        /// <summary>
        /// Schedule application start.
        /// </summary>
        /// <param name="scheduleAt">UTC time when to schedule application to start</param>
        /// <returns></returns>
        public static ConsoleAppOptions Schedule(DateTime scheduleAt) => new ConsoleAppOptions(TimeSpan.Zero, scheduleAt);

        #region Internal helpers

        internal bool ShouldPostpone(out TimeSpan postpone)
        {
            postpone = TimeSpan.Zero;

            if (_postponeInterval != TimeSpan.Zero)
                postpone = _postponeInterval;
            else if (_scheduleAt.HasValue  && _scheduleAt.Value > DateTime.UtcNow)
            {
                postpone = _scheduleAt.Value - DateTime.UtcNow;
            }

            return postpone != TimeSpan.Zero;
        }

        #endregion
    }
}
