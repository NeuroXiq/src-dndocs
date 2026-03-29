using DNDocs.Production.Tests.Shared;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DNDocs.Production.Tests.ProductionTests
{
    [TestFixture]
    public class ProductionTests
    {
        private ProductionTestsSettings settings;

        HttpClient httpClientDNDocs;
        HttpClient httpClientDNDocsDocs;
        HttpClient httpClientDNDocsJob;

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            try
            {
                httpClientDNDocs?.Dispose();
                httpClientDNDocsDocs?.Dispose();
                httpClientDNDocsJob?.Dispose();
            }
            catch (Exception)
            {
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var settingsJsonText = File.ReadAllText("./ProductionTestsSettings.json");
            var settings = System.Text.Json.JsonSerializer.Deserialize<ProductionTestsSettings>(settingsJsonText);

            this.settings = settings;

            httpClientDNDocs = new HttpClient();
            httpClientDNDocsDocs = new HttpClient();
            httpClientDNDocsJob = new HttpClient();

            httpClientDNDocs.BaseAddress = new Uri(settings.DNDocsServerUrl);
            httpClientDNDocsDocs.BaseAddress = new Uri(settings.DNDocsDocsServerUrl);
            httpClientDNDocsJob.BaseAddress = new Uri(settings.DNDocsJobServerUrl);
        }

        [Test]
        public async Task DNDocs_HealthCheck_Ok()
        {
            var result = await httpClientDNDocs.GetFromJsonAsync<HealthResult>("/system/health");

            Assert.That(result.AppName == "DNDocs.App");
            Assert.That(result.Online);
        }

        [Test]
        public async Task DNDocsDocs_HealthCheck_Ok()
        {
            var result = await httpClientDNDocsDocs.GetFromJsonAsync<HealthResult>("/system/health");

            Assert.That(result.AppName == "DNDocs.Docs");
            Assert.That(result.Online);
        }

        [Test]
        public async Task DNDocsJob_HealthCheck_Ok()
        {
            var result = await httpClientDNDocsJob.GetFromJsonAsync<HealthResult>("/system/health");

            Assert.That(result.Online);
        }

        [Test]
        public async Task DNDocs_LoadBasicPages_Ok()
        {
            Assert.Fail();
        }

        [Test]
        public async Task DNDocs_LoadBasicResources_Ok()
        {
            Assert.Fail();
        }

        class HealthResult
        {
            public bool Online { get; set; }
            public string AppName { get; set; }
        }
    }
}
