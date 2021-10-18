using System;

namespace ArtMind.AppFlow
{
    public class AppOptions
    {
        private readonly TimeSpan _postponeInterval;
        private readonly DateTime? _scheduleAt;

        private AppOptions(TimeSpan postpone, DateTime? schedule)
        {
            _postponeInterval = postpone;
            _scheduleAt = schedule;
        }

        internal static AppOptions Default => new AppOptions(TimeSpan.Zero, null);

        /// <summary>
        /// Postpone application start.
        /// </summary>
        /// <param name="postponeInterval">Flow will start after the postpone interval elapses, Default = TimeSpan.Zero, means no delay.</param>
        /// <returns></returns>
        public static AppOptions Postpone(TimeSpan postponeInterval) => new AppOptions(postponeInterval, null);

        /// <summary>
        /// Schedule application start.
        /// </summary>
        /// <param name="scheduleAt">UTC time when to schedule application to start</param>
        /// <returns></returns>
        public static AppOptions Schedule(DateTime scheduleAt) => new AppOptions(TimeSpan.Zero, scheduleAt);

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
