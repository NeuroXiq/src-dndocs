using System.Net.Http;

namespace DNDocs.ProductionTests.Shared
{
    public class ProductionTestsBase
    {
        protected ProductionTestsSettings Settings => GlobalOneTimeSetup.Settings;
        protected HttpClient HttpClientDNDocs => GlobalOneTimeSetup.HttpClientDNDocs;
        protected HttpClient HttpClientDNDocsDocs => GlobalOneTimeSetup.HttpClientDNDocsDocs;
        protected HttpClient HttpClientDNDocsJob => GlobalOneTimeSetup.HttpClientDNDocsJob;
    }
}
