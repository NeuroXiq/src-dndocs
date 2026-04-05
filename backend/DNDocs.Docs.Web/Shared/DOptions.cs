namespace DNDocs.Docs.Web.Shared
{
    public class DOptions
    {
        public string DataDirectory { get; set; }
        public StringsOpts Strings { get; set;  }
        public string DDocsApiKey { get; set; }
        public long MemoryCacheMaxSizeMB { get; set; }
        public string IndexNowApiKey { get; set; }

        public TimeSpan TimeSpanSaveMetrics { get; set; }
        public TimeSpan FlushAllLogsTimeSpan { get; set; }
        public TimeSpan TimespanGenerateSitemapPeriod { get; set; }
        
        public class StringsOpts
        {
            public string UrlNugetProjectGenerate { get; set; }
            public string UrlProjectNugetOrgFormat { get; set; }
            public string UrlDDocs { get; set; }
        }

        public string GetUrlDDocs(string relativePath) => string.Format(Strings.UrlDDocs, relativePath);

        public string GetUrlNugetProjectGenerate(string nugetPackageName, string nugetPackageVersion) =>
            string.Format(Strings.UrlNugetProjectGenerate, nugetPackageName, nugetPackageVersion);

        public string GetUrlNugetOrgProject(string nugetPackageName, string nugetPackageVersion, string path) =>
            string.Format(this.Strings.UrlProjectNugetOrgFormat, nugetPackageName, nugetPackageVersion, path);
    }
}
