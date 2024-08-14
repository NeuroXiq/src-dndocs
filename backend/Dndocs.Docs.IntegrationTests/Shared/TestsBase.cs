using DNDocs.Docs.Api.Client;
using DNDocs.Docs.IntegrationTests.Shared;
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
        public void Start_OneTimeSetupGlobalSetup()
        {
            try
            {
                OneTimeSetup();
            }
            catch (Exception e)
            {
                TestsBase.HardAbortAll = true;
                throw e;
            }
        }



        [OneTimeTearDown]
        public void Start_OneTimeGlobalTeardown()
        {
            ddocsProcess.Kill(true);
            ddocsProcess.Dispose();
        }

        static void CleanupInfrastructureFiles()
        {
            var itpath = TestsAppConfig.PathDDocsTestsInfrastructureDir;
            if (!(itpath?.EndsWith(@"\var\it-ddocs") == true))
            {
                TestsBase.HardAbortAll = true;
                throw new Exception("'!(itpath?.EndsWith(@\"\\var\\it-ddocs\") == true)': is this corrent? throwing for safe purpose before delete");
            }

            var files = Directory.GetFiles(TestsAppConfig.PathDDocsTestsInfrastructureDir).ToList();
            files.ForEach(File.Delete);
        }

        static void OneTimeSetup()
        {
            if (!File.Exists(TestsAppConfig.PathSmallSizeZip))
                throw new Exception($"Startup exception: small path site  file does not exists in '{TestsAppConfig.PathSmallSizeZip}'");

            if (!File.Exists(TestsAppConfig.PathBigSiteZip))
                throw new Exception($"Startup exception: big path site file does not exists in '{TestsAppConfig.PathBigSiteZip}'");

            CleanupInfrastructureFiles();
            StartServer();
        }

        static void StartServer()
        {
            // need to start real server to perform http requests on real environment
            // now starting with debug environment

            var existing = Process.GetProcessesByName("DNDocs.Docs.Web");
            if (existing.Length == 1) { existing[0].Kill(); }
            else if (existing.Length > 1) throw new Exception("startup exception, more than 1 process dndocs.docs.web found. unexpected");
            var startInfo = new ProcessStartInfo();

            // false -> want to this process be child of current proceess
            // to be destroyd after tests ends
            startInfo.UseShellExecute = false;
            startInfo.FileName = @"C:\Program Files\dotnet\dotnet.exe";
            startInfo.Arguments = $"run --project  \"{TestsAppConfig.PathDndocsDocsCsproj}\" --launch-profile IntegrationTests";
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            ddocsProcess = Process.Start(startInfo);

            // need to wait a second because 
            // sometimes tests start before app setup finish
            // question: when to know when app is ready to use?
            
            bool ok = false;
            var clientCheckAlive = TestsBase.CreateNewHttpClient();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Thread.Sleep(500);
                    clientCheckAlive.Public_Ping();
                    ok = true;
                } catch { }
            }

            if (!ok) throw new Exception("failed to ping or run ddocs.docs server after 5 seconds");

            //if (p.HasExited) throw new Exception("dndocs.docs failed to start, exited right after start");
            ObserveDndocsProcess(ddocsProcess);
        }

        static void ObserveDndocsProcess(Process process)
        {
            Task.Factory.StartNew((pObj) =>
            {
                Process p = pObj as Process;
                var outReader = p.StandardOutput;
                var outErrReader = p.StandardError;

                while (true)
                {
                    Thread.Sleep(1000);
                    var stdo = outReader.ReadToEnd();
                    var stderr = outErrReader.ReadToEnd();

                    Console.WriteLine("stdo: \r\n{0}\r\nstderr:\r\n{1}", stdo, stderr);
                }
            }, process);
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
            //return new FileStream(TestsAppConfig.DdocsHttpsUrl)
            return null;
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
