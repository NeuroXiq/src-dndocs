using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Newtonsoft.Json;
using DNDocs.Application.Shared;
using DNDocs.Api.Client;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.MyAccount;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.MyAccount;
using DNDocs.Api.Project;
using DNDocs.Api.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using DNDocs.Api.DTO;

namespace Web.IntegrationTests.Shared
{
    public class ApiSetupHelpers
    {
        private TestsBase testBase;

        public ApiSetupHelpers(TestsBase testBase)
        {
            this.testBase = testBase;
        }

        public CreateProjectResult CreateLightProject(string pname)
        {
            return CreateProject(pname, pname);
        }

        public class CreateProjectVersionResult
        {
            public CommandResultDto<int> CommandResult;
            public BgJobViewModel WaitingForBgJobResult;
        }

        //public CreateProjectVersionResult CreateProjectVersionByGitTag(int versioningId, string gitTag)
        //{ }

        public CreateProjectVersionResult CreateProjectVersionByGiTag(
            int versioningId,
            string tag)
        {
            var model = new CreateProjectVersionByGitTagModel(versioningId, tag);
            var cr = testBase.HttpPostCR<int>(Urls.Project.CreateProjectVersionByGitTag, model, null);
            BgJobViewModel bgResult = null;

            if (cr.Success)
            {
                bgResult = WaitForBgJob(cr.Result, DNDocs.Api.DTO.Enum.WaitingForBgJobType.CreateProject);
            }

            return new CreateProjectVersionResult
            {
                CommandResult = cr,
                WaitingForBgJobResult = bgResult
            };
        }

        public CreateProjectVersionResult CreateProjectVersionManually(
            int versioningId, string tag, NugetPackageModel[] nugetPackages)
        {
            var model = new CreateProjectVersionModel(versioningId, tag, nugetPackages);
            var cr = testBase.HttpPostCR<int>(Urls.Project.CreateProjectVersion, model, null);
            BgJobViewModel bgResult = null;

            if (cr.Success)
            {
                bgResult = WaitForBgJob(cr.Result, DNDocs.Api.DTO.Enum.WaitingForBgJobType.CreateProject);
            }

            return new CreateProjectVersionResult
            {
                CommandResult = cr,
                WaitingForBgJobResult = bgResult
            };
        }

        public int CreateLightProjectVersioning(string projectName)
        {
            var model = new CreateProjectVersioningModel(
                projectName,
                "https://github.com/NeuroXiq/IT-DNDocs-1",
                "it-dndocs-1",
                "https://github.com/NeuroXiq/IT-DNDocs-1.git",
                "main",
                "docs",
                "README.md",
                new List<NugetPackageModel>(),
                false);

            return testBase.HttpPostR<int>(Urls.Project.CreateProjectVersioning, model, System.Net.HttpStatusCode.OK, true);
        }

        public CreateProjectResult CreateProject(
            string projectName,
            string urlPrefix = "p-url",
            string description = "description",
            string githubUrl = "https://github.com/NeuroXiq/RobiniaDocs",
            string[] nugetPackages = null,
            bool nupkgAutorebuild = false,
            bool mdInclude = false,
            bool mdIncludeReadme = false,
            bool mdIncludeDocs = false,
            string githubMdRepoUrl = "",
            string githubMdBranchName = "",
            string githubMdRelativePathDocs = "",
            string githubMdRelativePathReadme = "",
            bool mdAutoRebuild = false)
        {
            var r = new CreateProjectResult();

            // no nupkg, no github md docs

            var model = new RequestProjectModel
            {
                ProjectName = projectName,
                UrlPrefix = urlPrefix,
                GithubUrl = githubUrl,
                Description = description,
                NugetPackages = nugetPackages ?? new string[0],
                GitMdRepoUrl = githubMdRepoUrl,
                GitMdBranchName = githubMdBranchName,
                GitMdRelativePathDocs = githubMdRelativePathDocs,
                GitMdRelativePathReadme = githubMdRelativePathReadme,
                PSAutoRebuild = mdAutoRebuild
            };

            // var jobid = testBase.HttpPostC<int>(Urls.Project.RequestProject, model).Result;
            var httpResult = HttpPostRequestProject(model);
            r.HttpResponse = httpResult;

            var json = httpResult.Content.ReadAsStringAsync().Result;

            if (!httpResult.IsSuccessStatusCode) return r;

            var result = JsonConvert.DeserializeObject<CommandResultDto<int>>(json);
            r.CommandResult = result;

            if (!result.Success) return null;

            var waitingResult = WaitForBgJob(result.Result, DNDocs.Api.DTO.Enum.WaitingForBgJobType.CreateProject);

            r.ProjectDto = waitingResult.CreatedProject;

            return r;
        }

        public HttpResponseMessage HttpPostRequestProject(RequestProjectModel m)
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(m.ProjectName ?? ""), nameof(m.ProjectName));
            form.Add(new StringContent(m.UrlPrefix ?? ""), nameof(m.UrlPrefix));
            form.Add(new StringContent(m.GithubUrl ?? ""), nameof(m.GithubUrl));
            form.Add(new StringContent(m.Description ?? ""), nameof(m.Description));
            
            form.Add(new StringContent(m.GitMdRepoUrl ?? ""), nameof(m.GitMdRepoUrl));
            form.Add(new StringContent(m.GitMdBranchName ?? ""), nameof(m.GitMdBranchName));
            form.Add(new StringContent(m.GitMdRelativePathDocs ?? ""), nameof(m.GitMdRelativePathDocs));
            form.Add(new StringContent(m.GitMdRelativePathReadme ?? ""), nameof(m.GitMdRelativePathReadme));
            form.Add(new StringContent(m.PSAutoRebuild.ToString()), nameof(m.PSAutoRebuild));

            foreach (var np in m.NugetPackages)
            {
                form.Add(new StringContent(np), nameof(m.NugetPackages));
            }

            var msg = new HttpRequestMessage(HttpMethod.Post, Urls.Project.RequestProject);
            msg.Content = form;

            return testBase.RawApiCall(msg);
        }

        internal void AssertProjectDocsHttpOk(string urlPrefix)
        {
            var indexHtmlUrl = testBase.AppSettings.ProjectDocsIndexUrl(urlPrefix);

            var result = testBase.httpClient.GetAsync(indexHtmlUrl).Result;

            Assert.True(result.IsSuccessStatusCode, "project docs request not success");
            Assert.That(
                result.Content.Headers.Contains("content-length") &&
                int.Parse(result.Content.Headers.GetValues("content-length").Single()) > 100,
                "Content length not exists or less than 101");
        }

        internal BgJobViewModel WaitForBgJob(int jobid, DNDocs.Api.DTO.Enum.WaitingForBgJobType type)
        {
            var waitModel = new WaitingForBgJobModel
            {
                BgJobId = jobid,
                Type = type
            };

            bool waiting = true;
            BgJobViewModel waitingResult = null;
            

            for (int i = 0; i < 60 && waiting; i++)
            {
                waitingResult = testBase.HttpPostR<BgJobViewModel>(Urls.MyAccount.WaitingForBgJob, waitModel);
                //waiting = !(waitingResult.CommandHandlerSuccess == true);
                throw new NotImplementedException();

                Thread.Sleep(1000);
            }

            if (waiting) Assert.Fail("Failed to wait for job, waiting too long");

            return waitingResult;
        }

        public class CreateProjectResult
        {
            public HttpResponseMessage HttpResponse;
            public ProjectDto ProjectDto;
            public CommandResultDto<int> CommandResult;
        }
    }
}
