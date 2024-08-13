using DNDocs.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Web.IntegrationTests.Shared;

namespace Web.IntegrationTests.Tests
{
    internal class AuthControllerTests: TestsBase
    {
        [Test]
        public void AdminLogin_WillRateLimit()
        {
            RestartServer();
            
            // arrange
            base.AuthType = AuthUserType.NoAuth;

            // act
            for (int i = 0; i < 3; i++)
            {
                HttpPostCR<string>(Urls.Auth.AdminLogin, "invalid-password", HttpStatusCode.BadRequest);
            }

            // assert
            HttpPostR<string>(Urls.Auth.AdminLogin, "invalid-pass", HttpStatusCode.TooManyRequests);
        }

        [Test]
        public void UserLogin_RateLimit()
        {
            RestartServer();

            // arrange
            AuthType = AuthUserType.NoAuth;

            // act
            for (int i = 0; i < 3; i++)
            {
                HttpPostCR<string>(Urls.Auth.CallbackGithubOAuth, " ", HttpStatusCode.BadRequest);
            }


            // assert
            HttpPostCR<string>(Urls.Auth.CallbackGithubOAuth, " ", HttpStatusCode.TooManyRequests);
        }
    }
}
