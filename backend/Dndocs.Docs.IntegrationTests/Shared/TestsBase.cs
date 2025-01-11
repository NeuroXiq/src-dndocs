using DNDocs.Docs.Api.Client;
using DNDocs.Docs.IntegrationTests.Shared;
using DNDocs.IntergrationTests.Shared;
using Microsoft.Extensions.Options;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


// this namespace is important because it 
// is ran under tests with given namespace.
// this namespace is root of all tests thus this will be called only once
namespace DNDocs.Docs.IntegrationTests
{
    
    [SetUpFixture]
    public class GlobalTestsSetup
    {
        private static Process ddocsProcess = null;


        [OneTimeSetUp]
        public async Task Start_OneTimeSetupGlobalSetup()
        {
            if (!File.Exists(TestsAppConfig.PathSmallSizeZip))
                throw new Exception($"Startup exception: small path site  file does not exists in '{TestsAppConfig.PathSmallSizeZip}'");

            if (!File.Exists(TestsAppConfig.PathBigSiteZip))
                throw new Exception($"Startup exception: big path site file does not exists in '{TestsAppConfig.PathBigSiteZip}'");

            ITSetup.StartServer_DDocs();
        }

        [OneTimeTearDown]
        public void Start_OneTimeGlobalTeardown()
        {
            ddocsProcess.Kill(true);
            ddocsProcess.Dispose();
        }

        
    }
}

namespace DNDocs.Docs.IntegrationTests.Shared
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
                new DDocsApiClientOptions(TestsAppConfig.ApiKey, TestsAppConfig.DdocsHttpsUrl));

            return clientIgnoreTlsCert;
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

        class DDocsApiOptions : IOptions<DDocsApiClientOptions>
        {
            public DDocsApiClientOptions Value { get; set; }

            public DDocsApiOptions(DDocsApiClientOptions options)
            {
                this.Value = options;
            }
        }
    }
}
