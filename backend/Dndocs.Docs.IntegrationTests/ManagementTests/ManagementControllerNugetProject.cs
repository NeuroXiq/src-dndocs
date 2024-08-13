using DNDocs.Docs.Api.Shared;
using DNDocs.Docs.IntegrationTests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Docs.IntegrationTests.ManagementTests
{
    internal class ManagementControllerNugetProject : TestsBase
    {
        [Test]
        public void Ping_WillThrowIfInvalidApiKey()
        {
            // ignore this is not needed in local env
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("x-api-key", "invalid");
            var result = client.GetAsync($"{base.TestServerUrl}/{DUrls.Management_Ping}").Result;

            AssertStatusCode(result.StatusCode, System.Net.HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task Ping_WillSucceessWithValidApiKey()
        {
            var result = await Client.Management_Ping("test - ping");

            Assert.That(result == "test - ping");
        }

        [Test] 
        public async Task CreateProject_WillNotCreateNugetProjectWithSamePackage()
        {
            var siteZip = base.GetSmallSiteFileStream();
            var samepkgname = NextPkgName;
            var samepkgver = NextPkgVer;

            await Client.Management_CreateProject(
                NextProjectId,
                NextProjectName,
                "metadata",
                null,
                null,
                samepkgver,
                samepkgver,
                3,
                GetSuperSmallSiteFileStream());


            Assert.That(() =>
            {
                var q = Client.Management_CreateProject(
                NextProjectId,
                "",
                NextProjectName,
                null,
                null,
                samepkgver,
                samepkgver,
                3,
                GetSuperSmallSiteFileStream()).Result;
            }, Throws.Exception);
        }

        [Test]
        public async Task CreateProject_WillCreateNugetProject()
        {
            var siteZip = base.GetSmallSiteFileStream();
            await Client.Management_CreateProject(
                NextProjectId,
                NextProjectName,
                null,
                null,
                null,
                "IT-NugetPkg123",
                "IT-NugetPkg-versoin1.2.3.4-suffix123-4",
                3,
                GetSuperSmallSiteFileStream());

        }

        [Test]
        public void CreateProject_WillNotCreateNugetProjectWithSamePackages()
        {
        
        }

        [Test]
        public void CreateProject_WillFailOnInvalidRequestsModelData()
        {
            
        }


    }
}
