
//using DNDocs.Domain.Entity.App;
//using DNDocs.Domain.ValueTypes;

//namespace DNDocs.Application.Commands.MyAccount
//{
//    public class CreateProjectCommandParams
//    {
//        public string ProjectName { get; set; }
//        public string GithubUrl { get; set; }
//        public string DocfxTemplate { get; set; }
//        public string Description { get; set; }
//        public string UrlPrefix { get; set; }
//        public NugetPackageDto[] NugetPackages { get; set; }

//        public string GitMdRepoUrl { get; set; }
//        public string GitMdBranchName { get; set; }
//        public string GitMdRelativePathDocs { get; set; }
//        public string GitMdRelativePathReadme { get; set; }
//        public bool PSAutorebuild { get; set; }

//        public ProjectType ProjectType { get; set; }
//        public string NugetPackageName { get; set; }
//        public string NugetPackageVersion { get; set; }

//        public CreateProjectCommandParams() { }

//        public CreateProjectCommandParams(
//            string projectName,
//            string githubUrl,
//            string docfxTemplate,
//            string description,
//            string robiniaUrlPrefix,
//            NugetPackageDto[] nugetPackages,
//            string mdDocsSourceGithubUrl,
//            string mdDocsGitBranchName,
//            string mdDocsGitRelativePathDocs,
//            string mdDocsGitRelativePathReadme,
//            bool mdDocsAutoRebuild,
//            ProjectType type,
//            string nugetPackageName,
//            string nugetPackageVersion
//            )
//        {
//            ProjectName = projectName;
//            GithubUrl = githubUrl;
//            DocfxTemplate = docfxTemplate;
//            Description = description;
//            UrlPrefix = robiniaUrlPrefix;
//            NugetPackages = nugetPackages;

//            GitMdRepoUrl = mdDocsSourceGithubUrl;
//            GitMdBranchName = mdDocsGitBranchName;
//            GitMdRelativePathDocs = mdDocsGitRelativePathDocs;
//            GitMdRelativePathReadme = mdDocsGitRelativePathReadme;
//            PSAutorebuild = mdDocsAutoRebuild;
//            ProjectType = type;
//            NugetPackageName = nugetPackageName;
//            NugetPackageVersion = nugetPackageVersion;
//        }
//    }
//}
