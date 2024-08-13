using Vinca.BufferLogger;

namespace DNDocs.Docs.Web.Model
{
    public class AppLog
    {
        public long Id { get; set; }
        public string? Message { get; set; }
        public string? CategoryName { get; set; }
        public int LogLevelId { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public DateTime Date { get; set; }

        public AppLog() { }

        public AppLog(string message,
            string categoryName,
            int logLevelId,
            int eventId,
            string eventName,
            DateTime date)
        {
            Message = message;
            CategoryName = categoryName;
            LogLevelId = logLevelId;
            EventId = eventId;
            EventName = eventName;
            Date = date;
        }

        internal static AppLog FromVLog(LogRow t)
        {
            return new AppLog(
                t.Message,
                t.CategoryName,
                (int)t.LogLevel,
                t.EventId.Id,
                t.EventId.Name,
                t.Date);
        }
    }
}
