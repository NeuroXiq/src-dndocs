namespace DNDocs.Job.Web.Shared
{
    public class DJobSettings
    {
        public string DNDocsJobApiKey { get; set; }
        public string DataDirectory { get; set; }
        public int MaxParallelBuildCount { get; set; }
    }
}
