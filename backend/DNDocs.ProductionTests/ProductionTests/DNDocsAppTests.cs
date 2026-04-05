using DNDocs.ProductionTests.Shared;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DNDocs.ProductionTests.ProductionTests
{
    [TestFixture]
    public class DNDocsAppTests : ProductionTestsBase
    {

        [Test]
        public async Task DNDocs_HealthCheck_Ok()
        {
            var result = await HttpClientDNDocs.GetFromJsonAsync<HealthResult>("/api/system/health");

            Assert.That(result.AppName == "DNDocs.App.Web", "result.AppName");
            Assert.That(result.Online, "result.Online");
            Assert.That(result.AppAssemblyInformationalVersion.Contains(Settings.EnvironmentName), "EnvironmentName");
        }

        [Test]
        public async Task DNDocs_LoadBasicPages_Ok()
        {
            var homepage = await HttpClientDNDocs.GetStringAsync("/");
            var generatePage = await HttpClientDNDocs.GetStringAsync("/generate/Arctium/1.0.11");

            Assert.That(homepage?.Length > 0, "homepage length > 0");
            Assert.That(
                homepage.Contains("<head>") &&
                homepage.Contains("<body>") &&
                homepage.Contains("<script") &&
                homepage.Contains("<form")
                , "homepage contains basic html attributes");

            Assert.That(
                homepage.Contains($"{Settings.DNDocsDocsServerUrl}/system/projects") &&
                homepage.Contains("https://github.com/NeuroXiq/DNDocs") &&
                homepage.Contains("https://github.com/NeuroXiq/src-dndocs") &&
                homepage.Contains("https://github.com/NeuroXiq/DNDocs/issues")
                , "homepage contains basic urls");
        }

        [Test]
        public async Task DNDocs_LoadBasicResources_Ok()
        {
            var favicon = await HttpClientDNDocs.GetByteArrayAsync("/favicon.ico");

            var stringAssets = new string[]
            {
                "/robots.txt",
                "/js-views/home/index.js",
                "/js-shared/main.js",
                "/js-shared/tools.js",
                "/css-views/home/index.css"
            };

            foreach (var path in stringAssets)
            {
                try
                {
                    var result = await HttpClientDNDocs.GetStringAsync(path);
                    Assert.That(result?.Length > 0, $"failed to load '{path}'");
                }
                catch (System.Exception e)
                {
                    Assert.Fail($"'{path}' http request failed. Exception: " + e.ToString());
                }
                
            }

            Assert.That(favicon?.Length > 10, "load favicon");
        }
    }
}
