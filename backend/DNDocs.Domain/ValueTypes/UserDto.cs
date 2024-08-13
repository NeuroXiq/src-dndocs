using DNDocs.Domain.Entity.App;

namespace DNDocs.Domain.ValueTypes
{
    public class UserDto
    {
        // todo somehow get rid of this and use from Domain.User (dto sholud not know this logins)
        public const string AdministratorUserLogin = "Administrator";
        public const string IntegrationTestsUser = "IntegrationTestsUser";

        public int Id { get; set; }
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

        public bool IsAdmin => Login == AdministratorUserLogin;

        public static UserDto Map(User user)
        {
            if (user == null) return null;

            var u = user;

            var result = new UserDto()
            {
                Id = u.Id,
                Login = u.Login,
                PrimaryEmail = u.PrimaryEmail,
                GithubPrimaryEmail = u.GithubPrimaryEmail,
                GithubId = u.GithubId,
                GithubLogin = u.GithubLogin,
                GithubReposUrl = u.GithubReposUrl,
                GithubUrl = u.GithubUrl,
                GithubHtmlUrl = u.GithubHtmlUrl,
                GithubAvatarUrl = u.GithubAvatarUrl,
                GithubType = u.GithubType
            };

            return result;
        }
    }
}
