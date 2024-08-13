using DNDocs.Api.Client;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.Enums;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.Project;
using DNDocs.Api.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Web.IntegrationTests.Shared;

namespace Web.IntegrationTests.Tests
{
    internal class ProjectControllerTests : TestsBase
    {
        [Test]
        public void CreateProject_WillSanitizeHtml()
        {
            // TODO
            // throw new Exception("TODO");
        }

        [Test]
        public void CreateProjectVersion_HardWillCreateProjectVersionFullGitClone()
        {
            var model = new CreateProjectVersioningModel(
                "dndocs-2",
                "https://github.com/NeuroXiq/IT-DNDocs-2",
                "itdocs-2",
                "https://github.com/NeuroXiq/IT-DNDocs-2.git",
                "main",
                "docs",
                "README.md",
                new List<NugetPackageModel>() { new NugetPackageModel() { IdentityId = "Arctium.Shared" } },
                false);


            int verid = HttpPostR<int>(Urls.Project.CreateProjectVersioning, model);

            ApiSetupHelper.CreateProjectVersionByGiTag(verid, "v1.1.1");
        }

        [Test]
        public void ProjectVersioning_NotOwnerCannotCreateVersionByTag()
        {
            var verid = ApiSetupHelper.CreateLightProjectVersioning("light-ver");

            base.AuthType = AuthUserType.TestUser2;

            var model = new CreateProjectVersionByGitTagModel(verid, "v1.1.1");


            HttpPostR<int>(Urls.Project.CreateProjectVersionByGitTag, model, HttpStatusCode.Forbidden);
        }

        [Test]
        public void ProjectVersioning_NotOwnerCannotAccessVersioningDetails()
        {
            var versioningId = ApiSetupHelper.CreateLightProjectVersioning("light-versing");

            base.AuthType = AuthUserType.TestUser2;

            // act & assert
            HttpGetQR<ProjectVersioningDto>(Urls.Project.FormatGetProjectVersioningById(versioningId), HttpStatusCode.Forbidden,  false);
        }

        [Test]
        public void ProjectVersioning_CreateVersionByTagReturnErrorIfTagNotExists()
        {
            var versioningId = ApiSetupHelper.CreateLightProjectVersioning("versioning-1");
            var model = new CreateProjectVersionByGitTagModel(versioningId, "NOT-EXISTS-TAG-NAME");

            // act & assert
            var result = HttpPostR<int>(Urls.Project.CreateProjectVersionByGitTag, model, HttpStatusCode.BadRequest, false);
        }

        [Test]
        public void CreateProjectVersion_WillCreateProjectVersionByGitTag()
        {
            var versioningId = ApiSetupHelper.CreateLightProjectVersioning("versioning-1");

            // act
            var result = ApiSetupHelper.CreateProjectVersionByGiTag(versioningId, "v2.2.2");
            Assert.IsTrue(result.CommandResult.Success);
            Assert.NotNull(result.WaitingForBgJobResult);
        }

        [Test]
        public void CreateProjectVersion_WillCreateProjectVersionManually()
        {
            // arrange
            var versioningId = ApiSetupHelper.CreateLightProjectVersioning("versioning-1");

            // act
            var result = ApiSetupHelper.CreateProjectVersionManually(versioningId, "v1.1.1", new NugetPackageModel[0]);
            
            // assert
            Assert.True(result.CommandResult.Success);
            Assert.NotNull(result.WaitingForBgJobResult);
        }

        [Test]
        public void CreateProjectVersioning_WillCreateVersioning()
        {
            // arrange & act & assert
            ApiSetupHelper.CreateLightProjectVersioning("versioning-1");
        }

        [Test]
        public void GetProjectById_WillReturnForbiddenIfNotOwnerOfProject()
        {
            base.AuthType = AuthUserType.TestUser1;
            var result = ApiSetupHelper.CreateLightProject("light");
            base.AuthType = AuthUserType.TestUser2;

            HttpPostCR<ProjectDto>(Urls.Project.GetProjectById, result.ProjectDto.Id, HttpStatusCode.Forbidden);
        }

        [Test]
        public void RequestProject_WillCreateThreeProjectsOneAfterAnother()
        {
            var result1 = ApiSetupHelper.CreateLightProject("light-1");
            var result2 = CreateHeavyProject();
            var result3 = ApiSetupHelper.CreateLightProject("light-2");

            var msg = "response status not ok, current: {0}";
            Assert.That(result1.HttpResponse.StatusCode == HttpStatusCode.OK, msg, result1.HttpResponse.StatusCode);
            Assert.That(result2.HttpResponse.StatusCode == HttpStatusCode.OK, msg, result2.HttpResponse.StatusCode);
            Assert.That(result3.HttpResponse.StatusCode == HttpStatusCode.OK, msg, result3.HttpResponse.StatusCode);

            ApiSetupHelper.AssertProjectDocsHttpOk("light-1");
            ApiSetupHelper.AssertProjectDocsHttpOk("heavy-proj");
            ApiSetupHelper.AssertProjectDocsHttpOk("light-2");

            RestartServer();
        }

        [Test]
        public void RequestAutoupgrade_WillRateLimit()
        {
            RestartServer();

            for (int i = 0; i < 20; i++)
            {
                var r = ApiSetupHelper.CreateLightProject($"invalid name just to rate limit !@#$ER");
                Assert.That(r.HttpResponse.StatusCode == HttpStatusCode.BadRequest);
            }

            var result  = ApiSetupHelper.CreateLightProject("light-rate-exceed");
            Assert.That(
                result.HttpResponse.StatusCode == HttpStatusCode.TooManyRequests,
                $"expected toomantrequest, but got: {result.HttpResponse.StatusCode}");

            RestartServer();
        }

        [Test]
        public void RequestAutoupgrade_WillSucceed()
        {
            // arrange & act & assert
            var result = ApiSetupHelper.CreateLightProject("light-autoupgrage");
            var httpResult = ApiCall<CommandResultDto>(Urls.Project.RequestAutoupgrade, HttpMethod.Post, result.ProjectDto.Id, HttpStatusCode.OK);
        }

        [Test]
        public void RequestAutoupgrade_WillFailIfProjectNotExists()
        {
            ApiCall<CommandResultDto>(Urls.Project.RequestAutoupgrade, HttpMethod.Post, 1234, HttpStatusCode.NotFound);
        }

        [Test]
        public void DeleteProject_WillSucceed()
        {
            // arrange
            var presult = ApiSetupHelper.CreateLightProject("light");

            // act
            HttpDelete(Urls.Project.DeleteProject, presult.ProjectDto.Id, HttpStatusCode.OK);
        }

        [Test]
        public void DeleteProject_WillFailOnEmptyDb()
        {
            // arrange & act & assert
            var result = base.HttpDelete(Urls.Project.DeleteProject, 1234, HttpStatusCode.NotFound, false);
        }

        [Test]
        public void RequestProject_WillFailOnInvalidParams()
        {
            var result = ApiSetupHelper.CreateLightProject("!@#!@##@!GV@ WGw3g'l;-");

            Assert.AreEqual(result.HttpResponse.StatusCode, HttpStatusCode.BadRequest);
        }

        [Test]
        public void RequestProject_CreateLightProjectSucceed()
        {
            var result = ApiSetupHelper.CreateLightProject("light-project");

            ApiSetupHelper.AssertProjectDocsHttpOk("light-project");

            Assert.True(result.HttpResponse.IsSuccessStatusCode, "not success status code");
            Assert.AreEqual(result.ProjectDto.UrlPrefix, "light-project");
            Assert.AreEqual(result.ProjectDto.Status, ProjectStatus.Active);
        }

        [Test]
        public void RequestProject_CreateLightProjectSucceed2()
        {
            var result = ApiSetupHelper.CreateLightProject("light-project");

            ApiSetupHelper.AssertProjectDocsHttpOk("light-project");

            Assert.True(result.HttpResponse.IsSuccessStatusCode, "not success status code");
            Assert.AreEqual(result.ProjectDto.UrlPrefix, "light-project");
            Assert.AreEqual(result.ProjectDto.Status, ProjectStatus.Active);
        }

        [Test]
        public void RequestProject_CreateHeavyProjectSucceed()
        {
            // arrange & act
            var result = CreateHeavyProject();

            // assert
            Assert.NotNull(result, "create heavy project failed, no result");
            Assert.AreEqual(result.ProjectDto.UrlPrefix, "heavy-proj");
            Assert.AreEqual(result.ProjectDto.Status, ProjectStatus.Active);
            ApiSetupHelper.AssertProjectDocsHttpOk("heavy-proj");

        }

        [Test]
        public async Task GetProjectById_ReturnsNullOnEmptyDb()
        {

            var r = await base.httpClient.GetAsync("/api/home/getallprojects");

            var res = await r.Content.ReadAsStringAsync();
        }

        private ApiSetupHelpers.CreateProjectResult CreateHeavyProject()
        {
            var result = ApiSetupHelper.CreateProject("heavy-proj",
                urlPrefix: "heavy-proj",
                nugetPackages: new string[] { "Arctium.Shared", "Arctium.Cryptography", "Arctium.Standards" },
                mdInclude: true,
                mdIncludeReadme: true,
                mdIncludeDocs: true,
                githubMdRepoUrl: "https://github.com/NeuroXiq/Arctium.git",
                githubMdBranchName: "master",
                githubMdRelativePathDocs: "docs",
                githubMdRelativePathReadme: "README.md");

            return result;
        }
    }
}
