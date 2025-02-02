using DNDocs.Application.Shared;
using DNDocs.Domain.ValueTypes;
using System.Text.Json;

namespace DNDocs.Application.Commands.Auth
{
    public class LoginUserCommand : Command<string>
    {
        public class GithubLoginCommand
        {
            public string GithubCode { get; set; }

            public GithubLoginCommand() { }
            public GithubLoginCommand(string code) { GithubCode = code; }
        }

        public class AdminLoginCommand
        {
            public string Login { get; set; }
            public string Password { get; set; }

            public AdminLoginCommand() { }
            public AdminLoginCommand(string login, string password)
            {
                Login = login;
                Password = password;
            }
        }

        public class TestUserLoginCommand
        {
            public string Login { get; set; }

            public TestUserLoginCommand(string login) { Login = login; }
        }

        public GithubLoginCommand GithubLogin { get; set; }
        public AdminLoginCommand AdminLogin { get; set; }
        public TestUserLoginCommand TestUserLogin { get; set; }

        public LoginUserCommand() { }

        public LoginUserCommand(GithubLoginCommand githubCommand, AdminLoginCommand adminLoginCommand)
        {
            GithubLogin = githubCommand;
            AdminLogin = adminLoginCommand;
        }

        // public GithubUserDto GithubUser { get; set; }
        // public OAuthAccessTokenDto GithubAccessToken { get; set; }
        // public bool IsAdministrator { get; set; }

        // public LoginUserCommand(
        //     // GithubUserDto githubUserDto,
        //     OAuthAccessTokenDto githubOauthAccesstoken)
        // {
        //     IsAdministrator = false;
        //     GithubUser = githubUserDto;
        //     GithubAccessToken = githubOauthAccesstoken;
        // }
        // 
        // public LoginUserCommand(bool isAdmin)
        // {
        //     if (!isAdmin) throw new Exception("ctor only for admin");
        // 
        //     IsAdministrator = true;
        // }

       
    }
}
