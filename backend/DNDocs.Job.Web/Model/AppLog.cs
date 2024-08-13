namespace DNDocs.Job.Web.Model
{
    public class AppLog
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public string CategoryName { get; set; }
        public int LogLevelId { get; set; }
        public DateTime Date { get; set; }
    }
}
