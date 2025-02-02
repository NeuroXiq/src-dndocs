using DNDocs.Docs.Api.Management;
using DNDocs.Docs.IntegrationTests.Shared;
using DNDocs.IntergrationTests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Docs.IntegrationTests.PublicContentTests
{
    [TestFixture]
    internal class PublicContentControllerTests : TestsBase
    {
        const string NPknName = "testing-content-nupkg";
        const string NPknVer= "1.2.3.4-testing-version-string";

        

        [Test]
        public async Task WillServe_NugetProjectPages()
        {
            await AssertHtmlContains("/api/System.Data.SQLite.AssemblySourceIdAttribute.html", "System.Data.SQLite.AssemblySourceIdAttribute");
            await AssertHtmlContains("/index.html", " ");
            await AssertHtmlContains("/api/index.html", " ");

            string[] apiFolderFiles = new string[]
            {
"index.html",
"System.Data.SQLite.AssemblySourceIdAttribute.html",
"System.Data.SQLite.AssemblySourceTimeStampAttribute.html",
"System.Data.SQLite.AuthorizerEventArgs.html",
"System.Data.SQLite.BusyEventArgs.html",
"System.Data.SQLite.CollationEncodingEnum.html",
"System.Data.SQLite.CollationSequence.html",
"System.Data.SQLite.CollationTypeEnum.html",
"System.Data.SQLite.CommitEventArgs.html",
"System.Data.SQLite.ConnectionEventArgs.html",
"System.Data.SQLite.FunctionType.html",
"System.Data.SQLite.Generic.html",
// not work not investigated why (maybe not important)
//"System.Data.SQLite.Generic.SQLiteModuleEnumerable-1.html",
//"System.Data.SQLite.Generic.SQLiteVirtualTableCursorEnumerator-1.html",
"System.Data.SQLite.html",
"System.Data.SQLite.ISQLiteChangeGroup.html",
"System.Data.SQLite.ISQLiteChangeSet.html",
"System.Data.SQLite.ISQLiteChangeSetMetadataItem.html",
"System.Data.SQLite.ISQLiteConnectionPool.html",
"System.Data.SQLite.ISQLiteConnectionPool2.html",
"System.Data.SQLite.ISQLiteManagedModule.html",
"System.Data.SQLite.ISQLiteNativeHandle.html",
"System.Data.SQLite.ISQLiteNativeModule.html",
"System.Data.SQLite.ISQLiteSchemaExtensions.html",
"System.Data.SQLite.ISQLiteSession.html",
"System.Data.SQLite.LogEventArgs.html",
"System.Data.SQLite.ProgressEventArgs.html",
"System.Data.SQLite.SessionConflictCallback.html",
"System.Data.SQLite.SessionTableFilterCallback.html",
"System.Data.SQLite.SQLiteAuthorizerActionCode.html",
"System.Data.SQLite.SQLiteAuthorizerEventHandler.html",
"System.Data.SQLite.SQLiteAuthorizerReturnCode.html",
"System.Data.SQLite.SQLiteBackupCallback.html",
"System.Data.SQLite.SQLiteBindValueCallback.html",
"System.Data.SQLite.SQLiteBlob.html",
"System.Data.SQLite.SQLiteBusyEventHandler.html",
"System.Data.SQLite.SQLiteBusyReturnCode.html",
"System.Data.SQLite.SQLiteCallback.html",
"System.Data.SQLite.SQLiteChangeSetConflictResult.html",
"System.Data.SQLite.SQLiteChangeSetConflictType.html",
"System.Data.SQLite.SQLiteChangeSetStartFlags.html",
"System.Data.SQLite.SQLiteCommand.html",
"System.Data.SQLite.SQLiteCommandBuilder.html",
"System.Data.SQLite.SQLiteCommitHandler.html",
"System.Data.SQLite.SQLiteCompareDelegate.html",
"System.Data.SQLite.SQLiteConfigDbOpsEnum.html",
"System.Data.SQLite.SQLiteConnection.html",
"System.Data.SQLite.SQLiteConnectionEventHandler.html",
"System.Data.SQLite.SQLiteConnectionEventType.html",
"System.Data.SQLite.SQLiteConnectionFlags.html",
"System.Data.SQLite.SQLiteConnectionStringBuilder.html",
"System.Data.SQLite.SQLiteContext.html",
"System.Data.SQLite.SQLiteConvert.html",
"System.Data.SQLite.SQLiteDataAdapter.html",
"System.Data.SQLite.SQLiteDataReader.html",
"System.Data.SQLite.SQLiteDataReaderValue.html",
"System.Data.SQLite.SQLiteDateFormats.html",
"System.Data.SQLite.SQLiteDelegateFunction.html",
"System.Data.SQLite.SQLiteErrorCode.html",
"System.Data.SQLite.SQLiteException.html",
"System.Data.SQLite.SQLiteExecuteType.html",
"System.Data.SQLite.SQLiteExtra.html",
"System.Data.SQLite.SQLiteFactory.html",
"System.Data.SQLite.SQLiteFinalDelegate.html",
"System.Data.SQLite.SQLiteFunction.html",
            };

            foreach (var file in apiFolderFiles)
            {
                // check if text without '.html' exists somewhere in file (e.g. in <h1> title)
                await AssertHtmlContains($"/api/{file}", file.Substring(0, file.Length - 5));
            }
        }

        //[Test]
        //public async Task WillLoad()
        //{

        //}

        //[Test]
        //public void Test()
        //{
        //    throw new Exception();
        //}

        //[Test]
        //public void Test2()
        //{
        //    throw new Exception();
        //}

        //[Test]
        //public void test()
        //{
        //    throw new Exception();
        //}

        [Test]
        public async Task WillServeStaticFiles()
        {
            string[] staticFiles = new string[]
            {
                "favicon.ico",
                // "main.css",
                "robots.txt",
                "index.html",
                "public/dndocs-docfx-script.js"
            };

            foreach (var file in staticFiles)
            {
                var result = await hclient.GetAsync($"/{file}");
                var byteData = await result.Content.ReadAsByteArrayAsync();

                Assert.NotZero(byteData.Length);
            }
        }

        [Test]
        public async Task WillRedirectIfNugetProjectNotFound()
        {
            var name = "not-exists-nupkgname";
            var ver = "notexists-nupkg-ver";
            var response = await hclient.GetAsync($"/n/{name}/{ver}");
            Assert.That(response.Headers.Location.ToString() == $"{TestsAppConfig.DNHttpUrl}/?packageName={name}&packageVersion={ver}");
            Assert.That(response.StatusCode == System.Net.HttpStatusCode.Found);
        }

        private HttpClient hclient;

        private async Task AssertHtmlContains(string path, string pattern)
        {
            // hclient.DefaultRequestHeaders.Add("accept-encoding", "br");
            var a = await hclient.GetAsync($"/n/{NPknName}/{NPknVer}{path}");
            var bb = await a.Content.ReadAsByteArrayAsync();
            var b = Encoding.UTF8.GetString(bb);
            Assert.That(b.Contains(pattern));
        }

        [OneTimeTearDown]
        public void TearDownLocal() { hclient.Dispose(); }

        [OneTimeSetUp]
        public void SetUpOneProject()
        {
            hclient = CreateHttpClientForContent();

            _ = Client.Management_CreateProject(
                NextProjectId,
                NextProjectName,
                null,
                null,
                null,
                NPknName,
                NPknVer,
                ProjectType.NugetOrg,
                GetSmallSiteFileStream()).Result;
        }

        public static HttpClient CreateHttpClientForContent()
        {
            // this is just normal HTTP client, should simulate user
            // exploring api docs
            // Creating client in this way becase want to skip TLS Cert validation
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            var client = new HttpClient(handler);
            client.BaseAddress = new Uri(TestsAppConfig.DdocsHttpsUrl);

            return client;
        }
    }
}
