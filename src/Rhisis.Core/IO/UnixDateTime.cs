using System;

namespace Rhisis.Core.IO
{
    /// <summary>
    /// Represent UNIX Epoch time.
    /// </summary>
    /// <remarks>
    /// For more information, see <see href="https://fr.wikipedia.org/wiki/Epoch"/>.
    /// </remarks>
    public static class UnixDateTime
    {
        private static readonly long StartupTicks = Environment.TickCount;
        private static readonly DateTime MinDate = new DateTime(1970, 1, 1);

        /// <summary>
        /// Gets the date time in seconds from a specified date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long ToSeconds(DateTime date)
        {
            if (date < MinDate)
                date = MinDate;

            return (long)(date - MinDate).TotalSeconds;
        }

        /// <summary>
        /// Gets the date time in seconds from now.
        /// </summary>
        /// <returns></returns>
        public static long NowToSeconds()
        {
            return ToSeconds(DateTime.UtcNow);
        }

        /// <summary>
        /// Gets the date time in miliseconds from a specified date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static double ToMiliseconds(DateTime date)
        {
            if (date < MinDate)
                date = MinDate;

            return (date - MinDate).TotalMilliseconds;
        }

        /// <summary>
        /// Gets the date time in milliseconds.
        /// </summary>
        /// <returns></returns>
        public static double NowToMiliseconds()
        {
            return ToMiliseconds(DateTime.UtcNow);
        }

        /// <summary>
        /// Gets the number of milliseconds elapsed since the program has started.
        /// </summary>
        /// <returns></returns>
        public static long GetElapsedTime()
        {
            return Environment.TickCount - StartupTicks;
        }
    }
}
