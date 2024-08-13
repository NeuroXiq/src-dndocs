using DNDocs.Domain.Entity.App;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Domain.ValueTypes.Project
{
    public struct CreateProjectParams
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
        public bool PSAutorebuild { get; set; }
        public int UserId { get; set; }
        public ProjectType ProjectType { get; set; }
        public string NugetOrgPackageName { get; set; }
        public string NugetOrgPackageVersion { get; set; }

        public CreateProjectParams(
            string projectName,
            string githubUrl,
            string docfxTemplate,
            string description,
            string robiniaUrlPrefix,
            IList<NugetPackageDto> nugetPackages,
            int userId,
            string mdDocsSourceGithubUrl,
            string mdDocsGitBranchName,
            string mdDocsGitRelativePathDocs,
            string mdDocsGitRelativePathReadme,
            string gitCommitHash,
            bool mdDocsAutoRebuild,
            ProjectType projectType,
            string nugetOrgPackageName,
            string nugetOrgPackageVersion
            )
        {
            ProjectName = projectName;
            GithubUrl = githubUrl;
            DocfxTemplate = docfxTemplate;
            Description = description;
            UrlPrefix = robiniaUrlPrefix;
            NugetPackages = nugetPackages;

            GitMdRepoUrl = mdDocsSourceGithubUrl;
            GitMdBranchName = mdDocsGitBranchName;
            GitMdRelativePathDocs = mdDocsGitRelativePathDocs;
            GitMdRelativePathReadme = mdDocsGitRelativePathReadme;
            GitDocsCommitHash = gitCommitHash;
            PSAutorebuild = mdDocsAutoRebuild;
            UserId = userId;
            ProjectType = projectType;
            NugetOrgPackageName = nugetOrgPackageName;
            NugetOrgPackageVersion = nugetOrgPackageVersion;

        }
    }
}
