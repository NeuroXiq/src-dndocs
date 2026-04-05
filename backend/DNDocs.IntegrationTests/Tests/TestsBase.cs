using DNDocs.Docs.Api.Client;
using DNDocs.IntergrationTests.Shared;
using Microsoft.Extensions.Options;
using System.Net;

namespace DNDocs.IntegrationTests.Shared
{
    [TestFixture]
    public class TestsBase
    {
        protected string TestServerUrl => TestsAppConfig.DdocsHttpsUrl;
        protected DDocsApiClient Client { get; private set; }
        public static bool HardAbortAll = false;

        // safe to get next project id not  used yet
        protected int NextProjectId => Interlocked.Increment(ref projectIdCounter);

        protected string NextProjectName => Guid.NewGuid().ToString();
        protected string NextPkgName => "pkgname-1.2.3.4-suffix" + Guid.NewGuid().ToString();
        protected string NextPkgVer => "pkgver-1.2.3.4-suffix" + Guid.NewGuid().ToString();

        static int projectIdCounter = 1;

        public TestsBase()
        {
            this.Client = CreateNewHttpClient();
        }

        public void AssertStatusCode(HttpStatusCode current, HttpStatusCode expected)
        {
            Assert.That(current == expected);
        }


        public Stream GetSuperSmallSiteFileStream()
        {
            return new FileStream(TestsAppConfig.PathSuperSmallSiteZip, FileMode.Open, FileAccess.Read);
        }

        public Stream GetSmallSiteFileStream()
        {
            return new FileStream(TestsAppConfig.PathSmallSizeZip, FileMode.Open, FileAccess.Read);
        }



        public static DDocsApiClient CreateNewHttpClient()
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

            client.BaseAddress = new Uri(TestsAppConfig.DdocsHttpsUrl);
            client.Timeout = TimeSpan.FromMinutes(1);

            var clientIgnoreTlsCert = new DDocsApiClient(
                new MockIOptions<DNDocsDocsApiClientOptions>(new DNDocsDocsApiClientOptions(TestsAppConfig.ApiKey, TestsAppConfig.DdocsHttpsUrl)));

            return clientIgnoreTlsCert;
        }

        class MockIOptions<T> : IOptions<T> where T : class
        {
            public T Value { get; private set; }

            public MockIOptions(T options)
            {

            }
        }

        [SetUp]
        public void SetUp()
        {
            if (HardAbortAll)
            {
                Assert.Inconclusive("Previous test failed");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (HardAbortAll)
            {
                Assert.Inconclusive("Previous test failed");
            }
        }

        class DDocsApiOptions : IOptions<DNDocsDocsApiClientOptions>
        {
            public DNDocsDocsApiClientOptions Value { get; set; }

            public DDocsApiOptions(DNDocsDocsApiClientOptions options)
            {
                this.Value = options;
            }
        }
    }
}
