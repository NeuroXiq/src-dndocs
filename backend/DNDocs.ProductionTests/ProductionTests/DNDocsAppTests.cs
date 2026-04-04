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
            Assert.Fail();
        }

        [Test]
        public async Task DNDocs_LoadBasicResources_Ok()
        {
            var favicon = await HttpClientDNDocs.GetByteArrayAsync("/favicon.ico");
            Assert.That(favicon?.Length > 10, "load favicon");
        }
    }
}
