using System.Text;

namespace DNDocs.Shared.Configuration
{
    public class DNDocsSettings
    {
        public GithubOAuthSettings GithubOAuth { get; set; }
        public JwtSettings Jwt { get; set; }
        public StringsSettings Strings { get; set; }
        public string DDocsApiKey { get; set; }
        public string DJobApiKey { get; set; }
        public string DNApiKey { get; set; }
        public string AdminPasswordSha512 { get; set; }

        public int BackendBackgroundWorkerDoImportantWorkSleepSeconds { get; set; }
        public int BackendBackgroundWorkerDoWorkSleepSeconds { get; set; }
        public int FrontendBackgroundWorkerDoWorkSleepSeconds { get; set; }

        public string OSPathInfrastructureDirectory { get; set; }

        public string GitExeFilePath { get; set; }
        public string[] CorsAllowedOrigins { get; set; }

        public string DDocsServerUrl { get; set; }

        public DNDocsSettings()
        {
            GithubOAuth = new GithubOAuthSettings();
        }

        public class JwtSettings
        {
            public string Issuer { get; set; }
            public string Audience { get; set; }
            public string SymmetricSecurityKey { get; set; }

            public byte[] GetBytes_SymmetricSecurityKey() => Encoding.ASCII.GetBytes(SymmetricSecurityKey);
        }

        public class GithubOAuthSettings
        {
            public string ClientId { get; set; }
            public string Secret { get; set; }
        }

        public class StringsSettings
        {
            public string UrlProjectSingletonApiFolder { get; set; }
            public string UrlProjectVersionApiFolder { get; set; }
            public string UrlProjectNugetOrgApiFolder { get; set; }
            public string UrlProjectAllVersionsList { get; set; }
        }

        public string ProjectDocsIndexUrl(string urlPrefix) => throw new Exception("remove this");
        public string GetUrlSingletonProject(string ulrPrefix) => string.Format(this.Strings.UrlProjectSingletonApiFolder, ulrPrefix);
        public string GetUrlVersionProject(string urlprefix, string version) =>
            string.Format(this.Strings.UrlProjectVersionApiFolder, urlprefix, version);
        public string GetUrlNugetOrgProject(string nugetPackageName, string nugetPackageVersion) =>
            string.Format(this.Strings.UrlProjectNugetOrgApiFolder, nugetPackageName, nugetPackageVersion);

        public string GetUrlProjectAllVersionsList(int id)
        {
            throw new NotImplementedException();
        }
    }
}
