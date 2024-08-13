namespace DNDocs.Job.Web.Shared
{
    public class DJobSettings
    {
        public string DJobApiKey { get; set; }
        public string OSPathInfrastructureDirectory { get; set; }
        public int MaxParallelBuildCount { get; set; }
        public string ConsoleToolsDllFilePath { get; set; }
        public string DNServerUrl { get; set; }
        public string DDocsApiKey { get; set; }
        public string DDocsServerUrl { get; set; }
        public string DNApiKey { get; set; }
        public int KestrelPort { get; set; }

        public StringsSettings Strings { get; set; }

        public class StringsSettings
        {
            public string DNDocs { get; set; }
            public string UrlDNDocsDocfJsScriptUrl { get; set; }
        }
    }
}
