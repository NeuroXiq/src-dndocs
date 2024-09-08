namespace DNDocs.Docs.Web.Shared
{
    public class DSettings
    {
        public DFileSystemOptions DFileSystemOptions { get; set; }
        public StringsOpts Strings { get; set;  }
        public string DDocsApiKey { get; set; }
        public long MemoryCacheMaxSizeMB { get; set; }
        // public string X509CertificatePemPath { get; set; }
        // public string X509CertificateKeyPemPath { get; set; }
        public string IndexNowApiKey { get; set; }

        public TimeSpan TimeSpanSaveMetrics { get; set; }
        public TimeSpan FlushAllLogsTimeSpan { get; set; }
        public TimeSpan TimespanGenerateSitemapPeriod { get; set; }
        
        public class StringsOpts
        {
            public string UrlNugetProjectGenerate { get; set; }
            public string UrlProjectSingletonFormat { get; set; }
            public string UrlProjectVersionFormat { get; set; }
            public string UrlProjectNugetOrgFormat { get; set; }
            public string UrlDDocs { get; set; }
        }

        public string GetUrlDDocs(string relativePath) => string.Format(Strings.UrlDDocs, relativePath);

        public string GetUrlNugetProjectGenerate(string nugetPackageName, string nugetPackageVersion) =>
            string.Format(Strings.UrlNugetProjectGenerate, nugetPackageName, nugetPackageVersion);

        public string GetUrlSingletonProject(string urlprefix, string path) =>
            string.Format(this.Strings.UrlProjectSingletonFormat, urlprefix, path);
        
        public string GetUrlVersionProject(string urlprefix, string version, string path) =>
            string.Format(this.Strings.UrlProjectVersionFormat, urlprefix, version);
        
        public string GetUrlNugetOrgProject(string nugetPackageName, string nugetPackageVersion, string path) =>
            string.Format(this.Strings.UrlProjectNugetOrgFormat, nugetPackageName, nugetPackageVersion, path);
    }
}
