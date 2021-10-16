using System;

namespace ArtMind.AppFlow
{
    public interface IAppOptions
    {
        /// <summary>
        /// Postpone application start. Flow will start after the postpone interval will elapse.
        /// Default = TimeSpan.Zero, means no delay.
        /// </summary>
        TimeSpan PostponeInterval { get; }
    }

    public class AppOptions: IAppOptions
    {
        public AppOptions(TimeSpan postpone)
        {
            PostponeInterval = postpone;
        }

        public static AppOptions Default => new AppOptions(TimeSpan.Zero);

        public TimeSpan PostponeInterval { get; private set; }
    }
}
