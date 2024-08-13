using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.ValueTypes;
using DNDocs.Domain.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Commands.Projects
{
    public class CreateSingletonProjectCommand : Command<int>
    {
        public string ProjectName { get; set; }
        public string GithubUrl { get; set; }
        public string DocfxTemplate { get; set; }
        public string Description { get; set; }
        public string UrlPrefix { get; set; }
        public IList<NugetPackageDto> NugetPackages { get; set; }
        public string GitMdBranchName { get; set; }
        public string GitDocsCommitHash { get; set; }
        public string GitMdRepoUrl { get; set; }
        public string GitMdRelativePathDocs { get; set; }
        public string GitMdRelativePathReadme { get; set; }
        public bool PSAutoRebuild { get; set; }
    }
}
