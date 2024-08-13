using DNDocs.Web.Models.Admin;
using DNDocs.Api.Client;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.IntegrationTests.Shared;

namespace Web.IntegrationTests.Tests
{
    internal class HomeControllerTests : TestsBase
    {
        public HomeControllerTests()
        {
        }

        [Test]
        public void GetAllProjects_WillGetCreatedProjects()
        {
            var result = ApiSetupHelper.CreateLightProject("light");

            var projs = HttpGetQR<IList<ProjectDto>>(Urls.Home.GetAllProjects, System.Net.HttpStatusCode.OK);

            Assert.AreEqual(projs.Count, 1);
            Assert.AreEqual(projs.First().Id, result.ProjectDto.Id);
        }

        [Test]
        public void Sitemap_WillGenerateCorrectSitemap()
        {
            var pr = ApiSetupHelper.CreateLightProject("light");

            base.AuthType = AuthUserType.Admin;
            HttpPostR<object>(Urls.Admin.DoBackgroundWorkNow, new DoBackgroundWorkNowModel { ForceGenerateSitemap = true });

            bool generated = false;

            // wait max 15 seconds to generate
            for (int i = 0; i < 15 && (!generated); i++)
            {
                var url = $"{Urls.Other.Sitemaps}/sitemap.xml";
                var response = RawApiCall(new HttpRequestMessage(HttpMethod.Get, url));

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;

                    generated = content.Contains("sitemap-project-1.xml");
                }

                Thread.Sleep(1000);
            }

            Assert.IsTrue(generated, "Expected to generate sitemap in 15 seconds but failed");
        }

        [Test]
        public void GetVersionInfo_WillReturnVersionInfo()
        {
            base.AuthType = AuthUserType.NoAuth;
            var result = RawApiCall(new HttpRequestMessage(HttpMethod.Get, Urls.Home.GetVersionInfo));
            var body = result.Content.ReadAsStringAsync().Result;

            Assert.True(result.IsSuccessStatusCode, "not success status code, current code: {0}", result.StatusCode);
            Assert.IsNotEmpty(body, "body is empty");
        }

        [Test]
        public void TryItCreateProject_WillRateLimit()
        {
            RestartServer();
            base.AuthType = AuthUserType.NoAuth;

            for (int i = 0; i < 5; i++)
            {
                HttpPostR<int>(Urls.Home.TryItCreateProject, new { nugetPackages = new string[] { "Arctium.Shared" } });
            }

            HttpPostR<int>(
                Urls.Home.TryItCreateProject,
                new { nugetPackages = new string[] { "Arctium.Shared" } },
                System.Net.HttpStatusCode.TooManyRequests);
        }

        [Test]
        public void TryItCreateProject_WillSucceed()
        {
            RestartServer();
            AuthType = AuthUserType.NoAuth;

            int jobid = HttpPostR<int>(Urls.Home.TryItCreateProject, new { nugetPackages = new string[] { "Arctium.Shared" } });
            var waitResult = ApiSetupHelper.WaitForBgJob(jobid, DNDocs.Api.DTO.Enum.WaitingForBgJobType.TryItCreateProject);

            throw new NotImplementedException();
            // Assert.True(waitResult.CommandHandlerSuccess, "command handler not success");
            Assert.NotNull(waitResult.CreatedProject, "createdproject property is null but job completed");
            ApiSetupHelper.AssertProjectDocsHttpOk(waitResult.CreatedProject.UrlPrefix);
        }

        public void TryItCreateProject_WillReturnErrorOnInvalidNupkg()
        {
            throw new Exception();
        }
    }
}
