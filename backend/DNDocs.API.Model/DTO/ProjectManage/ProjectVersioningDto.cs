using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.DTO.ProjectManage
{
    public class ProjectVersioningDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string ProjectWebsiteUrl { get; set; }
        public string GitDocsRepoUrl { get; set; }
        public string GitDocsBranchName { get; set; }
        public string GitDocsRelativePath { get; set; }
        public string GitHomepageRelativePath { get; set; }
        public string UrlPrefix { get; set; }
        public bool Autoupgrage { get; set; }
        public List<NugetPackageDto> NugetPackages { get; set; }

        public ProjectVersioningDto() { }

        public ProjectVersioningDto(
            int id,
            string projectName,
            string projectWebsiteUrl,
            string urlPrefix,
            string gitDocsRepoUrl,
            string gitDocsBranchName,
            string gitDocsRelativePath,
            string gitHomepageRelativePath,
            bool autoupgrade,
            List<NugetPackageDto> nugetPackages)
        {
            Id = id;
            ProjectName = projectName;
            ProjectWebsiteUrl = projectWebsiteUrl;
            UrlPrefix = urlPrefix;
            GitDocsRepoUrl = gitDocsRepoUrl;
            GitDocsBranchName = gitDocsBranchName;
            GitDocsRelativePath = gitDocsRelativePath;
            GitHomepageRelativePath = gitHomepageRelativePath;
            Autoupgrage = autoupgrade;
            NugetPackages = nugetPackages;
        }
    }
}
