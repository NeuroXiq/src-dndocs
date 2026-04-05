using System.Text;

namespace DNDocs.Shared.Configuration
{
    public class DNDocsSettings
    {
        public JwtSettings Jwt { get; set; }
        public string AdminPasswordSha512 { get; set; }

        public int BackendBackgroundWorkerDoImportantWorkSleepSeconds { get; set; }
        public int BackendBackgroundWorkerDoWorkSleepSeconds { get; set; }
        public int FrontendBackgroundWorkerDoWorkSleepSeconds { get; set; }
        public string DataDirectory { get; set; }
        public string UrlProjectNugetOrgApiFolder { get; set; }
        public string DNDocsApiKey { get; set; }
        public string DNDocsDocsServerUrl { get; set; }

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

        public string GetUrlNugetOrgProject(string nugetPackageName, string nugetPackageVersion) =>
            string.Format(UrlProjectNugetOrgApiFolder, nugetPackageName, nugetPackageVersion);
    }
}
