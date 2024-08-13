using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Job.Api.Management
{
    public class BuildProjectModel
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string UrlPrefix { get; set; }
        public string GithubUrl { get; set; }
        public string PVGitTag { get; set; }
        public string NugetOrgPackageName { get; set; }
        public string NugetOrgPackageVersion { get; set; }
        public ProjectType ProjectType { get; set; }
        public string GitMdBranchName { get; set; }
        public string GitDocsCommitHash { get; set; }
        public string GitMdRelativePathDocs { get; set; }
        public string GitMdRelativePathReadme { get; set; }
        public string GitMdRepoUrl { get; set; }
        public string DocfxTemplate { get; set; }
        public int? PVProjectVersioningId { get; set; }

        public List<NugetPackage> ProjectNugetPackages { get; set; }

        public class NugetPackage
        {
            public string IdentityId { get; set; }
            public string IdentityVersion { get; set; }

            public NugetPackage() { }

            public NugetPackage(string identityId, string identityVersion)
            {
                IdentityId = identityId;
                IdentityVersion = identityVersion;
            }
        }
    }

    public enum ProjectType
    {
        Singleton = 1,
        Version = 2,
        NugetOrg = 3,
    }
}
