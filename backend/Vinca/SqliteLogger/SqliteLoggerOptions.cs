using Vinca.SqliteLogger;

namespace Vinca.SqliteLogger
{
    public class SqliteLoggerOptions
    {
        /// <summary>
        /// How often save logs (10 seconds e.g.)
        /// </summary>
        public TimeSpan SaveTimerPeriod { get; set; } = TimeSpan.FromSeconds(10);

        public string SqliteDbPath { get; set; }

        /// <summary>
        /// how often check db to clear old logs (e.g. 12h)
        /// </summary>
        public TimeSpan ClearTimerPeriod { get; set; } = TimeSpan.FromHours(12);


        /// <summary>
        /// how long store logs in db
        /// </summary>
        public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(60);
    }
}
