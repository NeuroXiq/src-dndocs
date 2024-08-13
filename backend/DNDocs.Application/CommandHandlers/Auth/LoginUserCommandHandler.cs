using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DNDocs.Application.Commands.Auth;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using DNDocs.Shared.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DNDocs.Application.CommandHandlers.Auth
{
    internal class LoginUserCommandHandler : CommandHandlerA<LoginUserCommand, string>
    {
        private IHttpClientFactory httpClientFactory;

        public DNDocsSettings RobiniaSettings { get; }

        private IAppUnitOfWork unitOfWork;

        public LoginUserCommandHandler(
            IAppUnitOfWork unitOfWork,
            IOptions<DNDocsSettings> rsettings,
            IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            this.RobiniaSettings = rsettings.Value;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<string> Handle(LoginUserCommand command)
        {
            int result = -1;
            User user = null;

            //todo validate because user can be logged in now

            if (command.AdminLogin != null)
            {
                Validation.ThrowError(!AdminPasswordValid(command.AdminLogin.Password), "Invalid password");
                string login = string.IsNullOrWhiteSpace(command.AdminLogin.Login) ? User.AdministratorUserLogin : command.AdminLogin.Login;

                user = unitOfWork.GetSimpleRepository<User>()
                    .Query().Where(t => t.Login == login)
                    .Single();
            }
            else if (command.GithubLogin != null)
            {
                Validation.NotStringIsNullOrWhiteSpace(command.GithubLogin.GithubCode, "GithubCode");
                GetUserGithubLogin(command.GithubLogin.GithubCode, out var githubUser, out var githubOAuthAccessToken);

                var gh = githubUser;
                var at = githubOAuthAccessToken;
                Validation.ThrowFieldError(nameof(gh.Login), "Login empty", string.IsNullOrWhiteSpace(gh.Login));
                Validation.ThrowFieldError(nameof(gh.PrimaryEmail), "PrimaryEmail empty", string.IsNullOrWhiteSpace(gh.PrimaryEmail));
                Validation.ThrowFieldError(nameof(at.AccessToken), "Access token empty", string.IsNullOrEmpty(at.AccessToken));

                var userrepo = this.unitOfWork.GetSimpleRepository<User>();
                var existingUser = userrepo
                   .Query()
                   .Where(t => t.GithubId == gh.Id)
                   .FirstOrDefault();

                if (existingUser == null)
                {
                    var newUser = new User(gh.Login, gh.PrimaryEmail);
                    newUser.SetGithub(gh.PrimaryEmail, gh.Id, gh.Login, gh.ReposUrl, gh.Url, gh.HtmlUrl, gh.AvatarUrl, gh.Type);

                    userrepo.Create(newUser);

                    existingUser = newUser;
                }

                var accessToken = new OAuthAccessToken(existingUser.Id, at.AccessToken, at.TokenType, at.Scope);
                this.unitOfWork.GetSimpleRepository<OAuthAccessToken>().Create(accessToken);

                await unitOfWork.SaveChangesAsync();
                result = existingUser.Id;
                user = existingUser;
            }
            else if (command.TestUserLogin != null)
            {
                user = unitOfWork.GetSimpleRepository<User>()
                    .Query().Where(t => t.Login == command.TestUserLogin.Login)
                    .Single();
            }
            else
            {

                Validation.ThrowError(command.AdminLogin == null && command.GithubLogin == null && command.TestUserLogin == null,
                    "All login command are null, provide single command to login user");
            }

            var jwt = GenerateJwtToken(user);

            return jwt;
        }

        class GithubResponse
        {
            public string access_token { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
        }


        bool AdminPasswordValid(string password)
        {
            var inputPassString = password ?? "";
            byte[] inputPassHash;
            bool passValid = false;

            using (SHA512 shaM = new SHA512Managed())
            {
                inputPassHash = shaM.ComputeHash(Encoding.UTF8.GetBytes(inputPassString));
            }

            var inputPass = Convert.ToHexString(inputPassHash).Replace("-", "").ToLower();
            var validPass = this.RobiniaSettings.AdminPasswordSha512.ToLower();

            if (inputPass.Length == validPass.Length)
            {
                bool charsValid = true;

                for (int i = 0; i < validPass.Length; i++)
                    charsValid &= inputPass[i] == validPass[i];

                passValid = charsValid;
            }

            return passValid;
        }

        void GetUserGithubLogin(string code, out GithubUserDto githubUser, out OAuthAccessTokenDto githubOAuthAccessToken)
        {
            //todo move this to IGithubAPI

            var sc = new StringContent(
                JsonSerializer.Serialize(new
                {
                    client_id = this.RobiniaSettings.GithubOAuth.ClientId,
                    client_secret = this.RobiniaSettings.GithubOAuth.Secret,
                    code = code
                }),
                Encoding.UTF8,
                System.Net.Mime.MediaTypeNames.Application.Json);

            var httpClient = this.httpClientFactory.CreateClient();

            var r = new HttpRequestMessage(HttpMethod.Post,
                "https://github.com/login/oauth/access_token")
            {
                Headers =
                {
                    { "accept", "application/json" }
                },
                Content = sc
            };

            var response = httpClient.Send(r);

            // var response = await httpClient.PostAsync(
            //     "https://github.com/login/oauth/access_token",
            //     sc);

            var content = response.Content.ReadAsStringAsync().Result;
            var oauthResult = JsonSerializer.Deserialize<GithubResponse>(content);

            var getUserHttpMsg = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user")
            {
                Headers =
                {
                    { "Authorization", "Bearer " + oauthResult.access_token },
                    {  "user-agent", "node.js" }
                },
            };

            var getUserJsonResult = httpClient.Send(getUserHttpMsg).Content.ReadAsStringAsync().Result;

            var getEmailsMsg = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails")
            {
                Headers =
                {
                    // { "accept", "application/json" },
                    { "Authorization", "Bearer " + oauthResult.access_token },
                    {  "user-agent", "node.js" }
                },
            };

            var getEmailsJsonResult = httpClient.Send(getEmailsMsg).Content.ReadAsStringAsync().Result;
            var allEmailsJsonNode = JsonNode.Parse(getEmailsJsonResult).AsArray();
            var userGithubData = JsonNode.Parse(getUserJsonResult);

            string primaryEmail = null;

            foreach (var emailNode in allEmailsJsonNode)
            {
                var email = emailNode["email"].ToString();
                var verified = emailNode["verified"].ToString();
                var primary = emailNode["primary"].ToString();
                var visibility = emailNode["visibility"].ToString();

                if (primary == "true" && verified == "true")
                {
                    primaryEmail = email;
                    break;
                }
            }

            githubUser = new GithubUserDto(primaryEmail,
                userGithubData["id"].ToString(),
                userGithubData["login"].ToString(),
                userGithubData["repos_url"].ToString(),
                userGithubData["url"].ToString(),
                userGithubData["html_url"].ToString(),
                userGithubData["avatar_url"].ToString(),
                userGithubData["type"].ToString());

            githubOAuthAccessToken = new OAuthAccessTokenDto(oauthResult.access_token, oauthResult.scope, oauthResult.token_type);
        }

        string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.PrimaryEmail),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim("Robinia_UserId", user.Id.ToString()),
            };

            if (User.AdministratorUserLogin == user.Login)
            {
                claims.Add(new Claim("Robinia_IsAdmin", "true"));
            }

            var securityKey = new SymmetricSecurityKey(RobiniaSettings.Jwt.GetBytes_SymmetricSecurityKey());
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new JwtSecurityToken(
                RobiniaSettings.Jwt.Issuer,
                RobiniaSettings.Jwt.Audience,
                claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            return jwtToken;
        }
    }
}
