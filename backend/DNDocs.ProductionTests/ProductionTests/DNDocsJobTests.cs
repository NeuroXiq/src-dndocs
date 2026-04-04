using DNDocs.ProductionTests.Shared;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DNDocs.ProductionTests.ProductionTests
{
    [TestFixture]
    public class DNDocsJobTests : ProductionTestsBase
    {
        [Test]
        public async Task DNDocsJob_HealthCheck_Ok()
        {
            var result = await HttpClientDNDocsJob.GetFromJsonAsync<HealthResult>("/api/system/health");

            Assert.That(result.Online, "online = false");
            Assert.That(result.AppName == "DNDocs.Job.Web", "appname not match");
            Assert.That(result.AppAssemblyInformationalVersion.Contains(Settings.EnvironmentName), "EnvironmentName");
        }
    }
}
