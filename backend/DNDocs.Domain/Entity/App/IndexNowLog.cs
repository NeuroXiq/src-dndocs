namespace DNDocs.Domain.Entity.App
{
    public class IndexNowLog : Entity
    {
        public long SiteItemIdStart { get; set; }
        public long SiteItemIdEnd { get; set; }
        public bool Success { get; set; }
        public string LastException { get; set; }
        public DateTime LastSubmitDate { get; set; }
        public int SubmitAttemptCount { get; set; }

        public IndexNowLog() { }

        public IndexNowLog(
            long idStart,
            long idEnd, 
            bool success, 
            string lastException, 
            DateTime lastSubmitDate, 
            int submitAttemptCount)
        {
            SiteItemIdStart = idStart;
            SiteItemIdEnd = idEnd;
            Success = success;
            LastException = lastException;
            LastSubmitDate = lastSubmitDate;
            SubmitAttemptCount = submitAttemptCount;
        }
    }
}
