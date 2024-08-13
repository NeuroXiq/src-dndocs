using DNDocs.Domain.Entity.Shared;

namespace DNDocs.Domain.Entity.App
{
    public class User : Entity, ICreateUpdateTimestamp
    {
        public const string AdministratorUserLogin = "Administrator";
        public const string RobiniaAppServiceUserLogin = "robinia-app-service";
        public const string NuGetUserLogin = "nuget-org";
        public const string User1LoginIntegrationTests = "User1LoginIntegrationTests";
        public const string User2LoginIntegrationTests = "User2LoginIntegrationTests";

        public string Login { get; set; }
        public string PrimaryEmail { get; set; }

        public string GithubPrimaryEmail { get; set; }
        public string GithubId { get; set; }
        public string GithubLogin { get; set; }
        public string GithubReposUrl { get; set; }
        public string GithubUrl { get; set; }
        public string GithubHtmlUrl { get; set; }
        public string GithubAvatarUrl { get; set; }
        public string GithubType { get; set; }

        public List<SystemMessage> SystemMessages { get; private set; }
        public List<RefUserProject> RefUserProject { get; set; }
        public List<BgJob> CreatedBgJobs { get; private set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }

        public User(string login,
            string primaryEmail)
        {
            Login = login;
            PrimaryEmail = primaryEmail;
        }

        public void SetGithub(
            string primaryEmail,
            string id,
            string login,
            string reposurl,
            string url,
            string htmlurl,
            string avatarurl,
            string type)
        {
            GithubPrimaryEmail = primaryEmail;
            GithubId = id;
            GithubLogin = login;
            GithubReposUrl = reposurl;
            GithubUrl = url;
            GithubHtmlUrl = htmlurl;
            GithubAvatarUrl = avatarurl;
            GithubType = type;

            SystemMessages = new List<SystemMessage>();
        }
    }
}
