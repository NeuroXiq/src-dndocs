using DNDocs.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Entity.App
{
    public class ProjectVersioning : Entity
    {
        public string ProjectName { get; set; }
        public string ProjectWebsiteUrl { get; set; }
        public string UrlPrefix { get; set; }
        public string GitDocsRepoUrl { get; set; }
        public string GitDocsBranchName { get; set; }
        public string GitDocsRelativePath { get; set; }
        public string GitHomepageRelativePath { get; set; }
        public bool Autoupgrage { get; set; }
        public DateTime? LastAutoupgradeAt { get; set; }
        public string LastAutoupgradeError { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }

        public List<Project> Projects { get; set; }
        public List<NugetPackage> NugetPackages { get; set; }
        public List<SystemMessage> SystemMessages { get; set; }

        public ProjectVersioning()
        {
            NugetPackages = new List<NugetPackage>();
        }

        public ProjectVersioning(
            int userId,
            string projectName,
            string projectWebsiteUrl,
            string urlPrefix,
            string gitDocsRepoUrl,
            string gitDocsBranchName,
            string gitDocsRelativePath,
            string gitHomepageRelativePath,
            bool autoupgrade)
        {
            Validation.NotStringIsNullOrWhiteSpace(urlPrefix);
            Validation.NotStringIsNullOrWhiteSpace(projectName);

            UserId = userId;
            ProjectName = projectName;
            ProjectWebsiteUrl = projectWebsiteUrl;
            UrlPrefix = urlPrefix;
            GitDocsRepoUrl = gitDocsRepoUrl;
            GitDocsBranchName = gitDocsBranchName;
            GitDocsRelativePath = gitDocsRelativePath;
            GitHomepageRelativePath = gitHomepageRelativePath;
            Autoupgrage = autoupgrade;

            NugetPackages = new List<NugetPackage>();
        }

        public override string ToString()
        {
            return $"Project Versioning ({Id}) {ProjectName}";
        }
    }
}
