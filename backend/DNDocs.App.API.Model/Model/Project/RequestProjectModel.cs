using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.Project
{
    public class RequestProjectModel
    {
        public virtual string ProjectName { get; set; }
        public virtual string UrlPrefix { get; set; }
        public virtual string GithubUrl { get; set; }
        public virtual string DocfxTemplate { get; set; }
        public virtual string Description { get; set; }
        public virtual string[] NugetPackages { get; set; }
        public virtual string GitMdRepoUrl { get; set; }
        public virtual string GitMdBranchName { get; set; }
        public virtual string GitMdRelativePathDocs { get; set; }
        public virtual string GitMdRelativePathReadme { get; set; }
        public virtual bool PSAutoRebuild { get; set; }
    }
}
