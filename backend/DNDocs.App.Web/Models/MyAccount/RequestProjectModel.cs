using DNDocs.Web.Application.Validation;
using System.ComponentModel.DataAnnotations;

namespace DNDocs.Web.Models.MyAccount
{
    public class RequestProjectModel : DNDocs.Api.Project.RequestProjectModel, IValidatableObject
    {
        [Required(ErrorMessage = "ProjectNameRequired")]
        public override string ProjectName { get; set; }

        [Required(ErrorMessage = "FormFieldRequired")]
        public override string UrlPrefix { get; set; }

        [Required(ErrorMessage = "FormFieldRequired")]
        public override string GithubUrl { get; set; }
        public override string Description { get; set; }
        public override string[] NugetPackages { get; set; }

        public override string GitMdRepoUrl { get; set; }
        public override string GitMdBranchName { get; set; }
        public override string GitMdRelativePathDocs { get; set; }
        public override string GitMdRelativePathReadme { get; set; }
        public override bool PSAutoRebuild { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(GitMdRepoUrl))
            {
                if (string.IsNullOrWhiteSpace(GitMdRepoUrl))
                    yield return new ValidationResult("Empty or invalid value", new[] { nameof(GitMdRepoUrl) });
                if (string.IsNullOrWhiteSpace(GitMdBranchName))
                    yield return new ValidationResult("Empty branch name", new[] { nameof(GitMdBranchName) });
            }
        }
    }
}
