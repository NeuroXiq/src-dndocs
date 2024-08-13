namespace DNDocs.Domain.Entity.App
{
    public class AppLog : Entity
    {
        public string? Message { get; set; }
        public string? CategoryName { get; set; }
        public int LogLevelId { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        // public string? Exception { get; set; }
        public DateTime? Date { get; set; }
        public string TraceId { get; set; }

        public AppLog() { }
        
        public AppLog(
            string message,
            string categoryName,
            int logLevelId,
            int eventId,
            string eventName,
            DateTime date,
            string traceId)
        {
            Message = message;
            CategoryName = categoryName;
            LogLevelId = logLevelId;
            EventId = eventId;
            EventName = eventName;
            Date = date;
            TraceId = traceId;
        }
    }
}
