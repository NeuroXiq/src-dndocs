using DNDocs.ProductionTests.Shared;
using System;
using System.IO;
using System.Net.Http;

namespace DNDocs.ProductionTests
{
    [SetUpFixture]
    public class GlobalOneTimeSetup
    {
        public static ProductionTestsSettings Settings { get; private set; }

        public static HttpClient HttpClientDNDocs { get; private set;}
        public static HttpClient HttpClientDNDocsDocs { get; private set; }
        public static HttpClient HttpClientDNDocsJob { get; private set; }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            try
            {
                HttpClientDNDocs?.Dispose();
                HttpClientDNDocsDocs?.Dispose();
                HttpClientDNDocsJob?.Dispose();
            }
            catch (Exception)
            {
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            try
            {
                Console.WriteLine("OneTimeSetup started");
                var settingsJsonText = File.ReadAllText("settings.json");
                var settings = System.Text.Json.JsonSerializer.Deserialize<ProductionTestsSettings>(settingsJsonText);

                Settings = settings;

                HttpClientDNDocs = new HttpClient();
                HttpClientDNDocsDocs = new HttpClient();
                HttpClientDNDocsJob = new HttpClient();

                HttpClientDNDocs.BaseAddress = new Uri(settings.DNDocsServerUrl);
                HttpClientDNDocsDocs.BaseAddress = new Uri(settings.DNDocsDocsServerUrl);
                HttpClientDNDocsJob.BaseAddress = new Uri(settings.DNDocsJobServerUrl);
            }
            catch (Exception e)
            {
                Assert.Fail("OneTimeSetUp failed, Assert.Fail called. Exception: \n" + e.ToString());
            }
            
        }
    }
}
