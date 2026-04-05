using DNDocs.ProductionTests.Shared;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DNDocs.ProductionTests.ProductionTests
{
    [TestFixture]
    public class DNDocsDocsTests : ProductionTestsBase
    {
        [Test]
        public async Task DNDocsDocs_HealthCheck_Ok()
        {
            var result = await HttpClientDNDocsDocs.GetFromJsonAsync<HealthResult>("/api/system/health");

            Assert.That(result.AppName == "DNDocs.Docs.Web", "result.AppName");
            Assert.That(result.Online, "result.Online");
            Assert.That(result.AppAssemblyInformationalVersion.Contains(Settings.EnvironmentName), "EnvironmentName");
        }

        [Test]
        public async Task DNDocs_LoadBasicResources_Ok()
        {
            var favicon = await HttpClientDNDocsDocs.GetByteArrayAsync("/favicon.ico");
            Assert.That(favicon?.Length > 10, "load favicon");
        }
    }
}
