using DNDocs.Api.DTO.Enum;
using DNDocs.Api.DTO.Enums;

namespace DNDocs.Api.DTO.ProjectManage
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public Guid UUID { get; set; }
        public string ProjectName { get; set; }
        public string UrlPrefix { get; set; }
        public string Description { get; set; }
        public string GithubUrl { get; set; }
        public string Comment { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public DateTime? LastDocfxBuildTime { get; set; }
        public string LastDocfxBuildErrorLog { get; set; }
        public DateTime? LastDocfxBuildErrorDateTime { get; set; }
        public List<NugetPackageDto> ProjectNugetPackages { get; set; }
        public List<RefBlobDataProjectDto> RefBlobDataProject { get; set; }
        public BgProjectHealthCheckStatus? BgHealthCheckHttpGetStatus { get; set; }
        public DateTime? BgHealthCheckHttpGetDateTime { get; set; }
        public DateTime? NupkgAutorebuildLastDateTime { get; set; }

        public string GitMdRepoUrl { get; set; }
        public string GitMdBranchName { get; set; }
        public string GitMdRelativePathDocs { get; set; }
        public string GitMdRelativePathReadme { get; set; }
        public bool PSAutoRebuild { get; set; }
        public string GitDocsCommitHash { get; set; }
    }
}
