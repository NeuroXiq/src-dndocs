


using DNDocs.Domain.Entity.Shared;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Utils;

namespace DNDocs.Domain.Entity.App
{
    public enum ProjectType
    {
        Singleton = 1,
        Version = 2,
        NugetOrg = 3
    }

    public class Project : Entity, ICreateUpdateTimestamp
    {
        public string ProjectName { get; set; }
        public string UrlPrefix { get; set; }
        public string Description { get; set; }
        public string GithubUrl { get; set; }
        public string Comment { get; set; }
        public ProjectState State { get; set; }
        public ProjectStateDetails StateDetails { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public DateTime? LastBuildStartOn { get; set; }
        public DateTime? LastBuildCompletedOn { get; set; }
        public string LastBuildErrorLog { get; set; }
        public List<RefUserProject> RefUserProject { get; set; }
        public List<NugetPackage> ProjectNugetPackages { get; set; }
        public BgProjectHealthCheckStatus? BgHealthCheckHttpGetStatus { get; set; }
        public DateTime? BgHealthCheckHttpGetDateTime { get; set; }
        public DateTime? NupkgAutorebuildLastDateTime { get; set; }
        public List<SystemMessage> SystemMessages { get; protected set; }

        public string DocfxTemplate { get; set; }

        public string GitMdRepoUrl { get; set; }
        public string GitMdBranchName { get; set; }
        public string GitMdRelativePathDocs { get; set; }
        public string GitMdRelativePathReadme { get; set; }
        public string GitDocsCommitHash { get; set; }

        public int? PVProjectVersioningId { get; set; }
        public ProjectVersioning PVProjectVersioning { get; set; }
        public string PVGitTag { get; set; }

        public string NugetOrgPackageName { get; set; }
        public string NugetOrgPackageVersion { get; set; }
        public ProjectType ProjectType { get; set; }

        public bool PSAutorebuild { get; protected set; }

        public Project()
        {
            RefUserProject = new List<RefUserProject>();
            ProjectNugetPackages = new List<NugetPackage>();
            SystemMessages = new List<SystemMessage>();
        }

        public Project(
            string projectName,
            string robiniaUrlPrefix,
            string description,
            string githubUrl,
            string docfxTemplate,
            ProjectState state,
            string comment,
            string mdDocsSourcegithubUrl,
            string mdDocsGitBranchName,
            string mdDocsGitRelativePathDocs,
            string mdDocsGitRelativePathReadme,
            string gitDocsCommitHash,
            bool psAutorebuild,
            ProjectType projectType,
            string nugetOrgPackageName,
            string nugetOrgPackageVersion) : this()
        {
            ProjectName = projectName;
            UrlPrefix = robiniaUrlPrefix;
            DocfxTemplate = docfxTemplate;
            Description = description;
            GithubUrl = githubUrl;
            State = state;
            LastBuildStartOn = DateTime.Now.AddYears(-50);
            ProjectType = projectType;
            NugetOrgPackageName = nugetOrgPackageName;
            NugetOrgPackageVersion = nugetOrgPackageVersion;


            Validation.EnumDefined(projectType, "ProjectType");

            if (!string.IsNullOrWhiteSpace(mdDocsSourcegithubUrl))
            {
                Validation.NotStringIsNullOrWhiteSpace(mdDocsGitBranchName);
                Validation.NotStringIsNullOrWhiteSpace(gitDocsCommitHash);
            }
            else
            {
                Validation.FieldErrorP(!string.IsNullOrWhiteSpace(mdDocsGitBranchName), mdDocsGitBranchName, "Must be empty if repository not provided");
                Validation.FieldErrorP(!string.IsNullOrWhiteSpace(mdDocsGitRelativePathDocs), mdDocsGitRelativePathDocs, "Must be empty if repository not provided");
                Validation.FieldErrorP(!string.IsNullOrWhiteSpace(mdDocsGitRelativePathReadme), mdDocsGitRelativePathReadme, "Must be empty if repository not provided");
                Validation.FieldErrorP(!string.IsNullOrWhiteSpace(gitDocsCommitHash), gitDocsCommitHash, "Must be empty if repository not provided");
            }

            GitMdRepoUrl = mdDocsSourcegithubUrl;
            GitMdBranchName = mdDocsGitBranchName;
            GitMdRelativePathDocs = mdDocsGitRelativePathDocs;
            GitMdRelativePathReadme = mdDocsGitRelativePathReadme;
            GitDocsCommitHash = gitDocsCommitHash;
            PSAutorebuild = psAutorebuild;
        }

        //public void AddSystemMessage(SystemMessageLevel level, string title, string msg, int? traceBgJobId)
        //{
        //    msg = $"Project: {this}\r\nCurrent Project Status: {Status}\r\n{msg}";

        //    Validation.ArgStringNotEmpty(title, nameof(title));
        //    Validation.ArgStringNotEmpty(msg, nameof(msg));
        //    Validation.EnumDefined(level, nameof(level));

        //    var sysMsg = new SystemMessage(SystemMessageType.Project, level, title, msg, DateTime.UtcNow, traceBgJobId);
        //    sysMsg.Project = this;

        //    SystemMessages.Add(sysMsg);
        //}

        public void AddUser(User user)
        {
            Validation.ArgNotNull(user, nameof(user));
            this.RefUserProject.Add(new RefUserProject(user, this));
        }

        public override string ToString()
        {
            return $"Project Id: {Id} Name: {ProjectName ?? ""}";
        }

        public static void ValidateUrlPrefix(string urlPrefix, string fieldName)
        {
            Validation.ThrowError(string.IsNullOrWhiteSpace(urlPrefix), "Null or empty url prefix");
            Validation.ThrowError(urlPrefix.Length < 3 || urlPrefix.Length > 128, "url prefix length must be in range 3 - 128");
            Validation.ThrowError(NormalizeUrlPrefix(urlPrefix) != urlPrefix, "Invalid chars in url prefix, valid chars: (a-z), (A-Z), (0-9), (-), (.)");
        }

        public static string ProjectNameForVersioning(string projectName, string gitTag)
        {
            return $"{projectName} {gitTag}";
        }

        public static string NormalizeUrlPrefix(string name)
        {
            string result = "";
            
            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                bool validChar = 
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    c == '.' || c == '-';

                result += validChar ? c : '-';
            }

            result = result.Trim();

            // if (result.Length > 64) result = result.Substring(0, 64);

            return result;
        }

        public static string NugetUserProjectName(string packageName, string packageVersion) => $"nuget-org-{packageName?.Trim()}-{packageVersion.Trim()}";

        internal static string UrlPrefixForVersion(string urlPrefix, string pVGitTag)
        {
            urlPrefix = urlPrefix.Trim();
            pVGitTag = pVGitTag.Trim();

            return NormalizeUrlPrefix($"{urlPrefix}-{pVGitTag}");
        }
    }
}
