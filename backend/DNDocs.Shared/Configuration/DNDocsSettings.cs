using System.Text;

namespace DNDocs.Shared.Configuration
{
    public class DNDocsSettings
    {
        public JwtSettings Jwt { get; set; }
        public string DNApiKey { get; set; }
        public string DJobApiKey { get; set; }
        public string DDocsApiKey { get; set; }
        public string AdminPasswordSha512 { get; set; }

        public int BackendBackgroundWorkerDoImportantWorkSleepSeconds { get; set; }
        public int BackendBackgroundWorkerDoWorkSleepSeconds { get; set; }
        public int FrontendBackgroundWorkerDoWorkSleepSeconds { get; set; }
        
        public string OSPathInfrastructureDirectory { get; set; }
        public string[] CorsAllowedOrigins { get; set; }

        public string UrlProjectNugetOrgApiFolder { get; set; }

        public DNDocsSettings()
        {
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

        public string GetUrlNugetOrgProject(string nugetPackageName, string nugetPackageVersion) =>
            string.Format(UrlProjectNugetOrgApiFolder, nugetPackageName, nugetPackageVersion);
    }
}
