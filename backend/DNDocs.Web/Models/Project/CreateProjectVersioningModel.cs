using DNDocs.Web.Resources;
using DNDocs.Api.Shared;
using System.ComponentModel.DataAnnotations;

namespace DNDocs.Web.Models.Project
{
    public class CreateProjectVersioningModel : DNDocs.Api.Project.CreateProjectVersioningModel
    {
        [Required(ErrorMessage = "FormFieldRequired")]
        public override string ProjectName { get; set; }
        
        [Required(ErrorMessage = "FormFieldRequired")]
        public override string ProjectWebsiteUrl { get; set; }

        public override string GitDocsRepoUrl { get; set; }
        public override string GitDocsBranchName { get; set; }
        public override string GitDocsRelativePath { get; set; }
        public override string GitHomepageRelativePath { get; set; }
        public override IList<NugetPackageModel> NugetPackages { get; set; }
        public override bool Autoupgrage { get; set; }

        public CreateProjectVersioningModel() { }
    }
}
