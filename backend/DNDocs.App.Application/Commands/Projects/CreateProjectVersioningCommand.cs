using DNDocs.Application.Shared;
using DNDocs.Api.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Projects
{
    public class CreateProjectVersioningCommand : Command<int>
    {
        public string ProjectName { get; set; }
        public string ProjectWebsiteUrl { get; set; }
        public string UrlPrefix { get; set; }
        public string GitDocsRepoUrl { get; set; }
        public string GitDocsBranchName { get; set; }
        public string GitDocsRelativePath { get; set; }
        public string GitHomepageRelativePath { get; set; }
        public IList<NugetPackageModel> NugetPackages { get; set; }
        public bool Autoupgrage { get; set; }

        public CreateProjectVersioningCommand()
        { }

        public CreateProjectVersioningCommand(
            string projectName,
            string projectWebsiteUrl,
            string urlPrefix,
            string gitDocsRepoUrl,
            string gitDocsBranchName,
            string gitDocsRelativePath,
            string gitHomepageRelativePath,
            IList<NugetPackageModel> nugetPackages,
            bool autoupgrade)
        {
            ProjectName = projectName;
            ProjectWebsiteUrl = projectWebsiteUrl;
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
