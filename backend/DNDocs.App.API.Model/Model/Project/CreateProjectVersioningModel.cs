using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.Project
{
    public class CreateProjectVersioningModel
    {
        public virtual string ProjectName { get; set; }
        public virtual string ProjectWebsiteUrl { get; set; }
        public virtual string UrlPrefix { get; set; }
        public virtual string GitDocsRepoUrl { get; set; }
        public virtual string GitDocsBranchName { get; set; }
        public virtual string GitDocsRelativePath { get; set; }
        public virtual string GitHomepageRelativePath { get; set; }
        public virtual IList<NugetPackageModel> NugetPackages { get; set; }
        public virtual bool Autoupgrage { get; set; }

        public CreateProjectVersioningModel() { }
        public CreateProjectVersioningModel(
           string projectName,
           string websiteUrl,
           string urlPrefix,
           string gitDocsRepoUrl,
           string gitDocsBranchName,
           string gitDocsRelativePath,
           string gitHomepageRelativePath,
           IList<NugetPackageModel> nugetPackages,
           bool autoupgrade)
        {
            ProjectName = projectName;
            ProjectWebsiteUrl = websiteUrl;
            UrlPrefix = urlPrefix;
            GitDocsRepoUrl = gitDocsRepoUrl;
            GitDocsBranchName = gitDocsBranchName;
            GitDocsRelativePath = gitDocsRelativePath;
            GitHomepageRelativePath = gitHomepageRelativePath;
            NugetPackages = nugetPackages;
            Autoupgrage = autoupgrade;
        }
    }
}
