using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework.Interfaces;
using DNDocs.Application.Application;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Infrastructure.Utils;
using DNDocs.Shared.Configuration;
using DNDocs.Shared.Utils;
using DNDocs.Api.Client;
using DNDocs.Api.DTO;
using DNDocs.Api.Admin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Web.IntegrationTests.Shared
{
    [TestFixture]
    public class TestsBase
    {
        public enum AuthUserType
        {
            Admin,
            TestUser1,
            TestUser2,
            NoAuth
        }

        public AppApplicationFactory AppFactory { get; private set; }
        public HttpClient httpClient;
        static string JwtAdminToken = null;
        static string JwtTestUser1Token = null;
        static string JwtTestUser2Token = null;
        const string SqliteExe = "C:\\my-files\\programs\\sqlite3\\sqlite3.exe";

        static string AppDbFilePath = null;
        static string EmptyDbFilePath = null;
        static bool GlobalOneTimeSetup = false;
        protected AuthUserType AuthType;
        protected ApiSetupHelpers ApiSetupHelper { get; private set; }
        public string ApiBaseUri => httpClient.BaseAddress.ToString();
        public DNDocsSettings AppSettings { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            RestartServer();
            if (!GlobalOneTimeSetup)
            {
                GlobalOneTimeSetup = true;

                var infrastructureFolder = AppFactory.Services.GetService<IOptions<DNDocsSettings>>()
                    .Value.OSPathInfrastructureDirectory;

                // stop server
                StopServer();

                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                
                var infrastructureDir = Path.Combine(infrastructureFolder, "robinia-infrastructure-files/");
                if (Directory.Exists(infrastructureDir))
                {
                    var allFiles = Directory.GetFiles(infrastructureDir, "*.*", SearchOption.AllDirectories);
                    // some files are read-only and cannot delete directory
                    // mark as normal
                    foreach (var filePath in allFiles) File.SetAttributes(filePath, FileAttributes.Normal);

                    Directory.Delete(infrastructureDir, true);
                }

                // restart server - will create new database because deleted above
                RestartServer();

                JwtAdminToken = HttpPostR<string>(Urls.Auth.AdminLogin, new AdminLoginModel(User.AdministratorUserLogin, "Rob1n1@"));
                JwtTestUser1Token = HttpPostR<string>(Urls.Auth.AdminLogin, new AdminLoginModel(User.User1LoginIntegrationTests, "Rob1n1@"));
                JwtTestUser2Token = HttpPostR<string>(Urls.Auth.AdminLogin, new AdminLoginModel(User.User2LoginIntegrationTests, "Rob1n1@"));

                AppDbFilePath = Path.Combine(infrastructureDir, "app/appdb.sqlite").Replace("\\", "/");
                EmptyDbFilePath = Path.Combine(infrastructureFolder, "integrationtests-empty-db.sqlite").Replace("\\", "/");

                if (File.Exists(EmptyDbFilePath))
                {
                    File.Delete(EmptyDbFilePath);
                }

                // prepare empty db for it tests:
                // 1. copy empty db as backup (restore from file instead of each time running migrations and create full infrastructure etc.)
                File.Copy(AppDbFilePath, EmptyDbFilePath);

                // 2. to speed up tests, instead of full 'git clone'
                // use cached repository in db (because will need to do real 'git clone' each  time test runs)
                string itGitStoreZipPath = Path.Combine(infrastructureFolder, "it-gitstore/E5DA57BE-898D-43E4-A9F3-80948FF3FDEC.zip").Replace("/", "\\");
                string destGitStore = Path.Combine(infrastructureDir, "gitstore").Replace("/", "\\");
                System.IO.Compression.ZipFile.ExtractToDirectory(itGitStoreZipPath, destGitStore);
                var now = DateTime.UtcNow.ToString("o");
                System.Diagnostics.Process.Start(
                    SqliteExe,
                    $"{EmptyDbFilePath} " +
                    $"\"insert into git_repo_store(uuid, git_repo_url, created_on, last_modified_on, last_access_on) " +
                    $"values ('E5DA57BE-898D-43E4-A9F3-80948FF3FDEC', 'https://github.com/neuroxiq/it-dndocs-1', '{now}', '{now}',  '{now}')\" .quit");


                ClearDB();
            }

            if (string.IsNullOrWhiteSpace(JwtAdminToken) ||
                string.IsNullOrWhiteSpace(JwtTestUser1Token) ||
                string.IsNullOrWhiteSpace(JwtTestUser2Token))
            {
                throw new Exception("FATAL IT EXCEPTION: Aborting all tests - cannot athorize");
            }

            ApiSetupHelper = new ApiSetupHelpers(this);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            StopServer();
        }

        void StopServer()
        {
            // hard to  determine when appfactory.dispose() called and if still available
            // need to manually  'stop' this service because strange problems with 'disposed' exceptions in application
            try { AppFactory?.Services.GetRequiredService<ApiBackgroundWorker>().StopAsync(default(CancellationToken)).Wait(); } catch { }
            AppFactory?.Dispose();
        }

        public void RestartServer()
        {
            StopServer();

            AppFactory = new AppApplicationFactory();
            httpClient = AppFactory.CreateClient();
            AuthType = AuthUserType.TestUser1;
            AppSettings = AppFactory.Services.GetRequiredService<IOptions<DNDocsSettings>>().Value;
        }

        void ClearDB()
        {
            // restore empty db before each test
            var arg = $"{AppDbFilePath}" + $" \".restore {EmptyDbFilePath}\" .quit";

            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = SqliteExe,
                    Arguments = arg,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            p.Start();

            for (int i = 0; i < 50 && (!p.HasExited); i++)
            { p.WaitForExit(100); p.Refresh(); }

            var exitcode = p.ExitCode;
            var a = p.StandardError.ReadToEnd();
            var b = p.StandardOutput.ReadToEnd();

            try { p.Kill(); } catch { }

            if (exitcode != 0)
            {
                throw new IEException("cannot restore empty db, restore process failed, exit code not zero");
            }
        }

        [SetUp]
        public void Setup()
        {
            AuthType = this.DefaultAuthUser();
            ClearDB();
        }

        protected virtual AuthUserType DefaultAuthUser()
        {
            return AuthUserType.TestUser1;
        }

        public TResult HttpPostR<TResult>(string url, object content, HttpStatusCode? assertStatusCode = HttpStatusCode.OK, bool assertCommandSuccess = false)
        {
            var cr = ApiCall<CommandResultDto<TResult>>(url, HttpMethod.Post, content, assertStatusCode);

            Assert.True(!assertCommandSuccess || cr.Success, $"HttpPost command result is not success, url: {url}");

            return cr.Result;
        }

        public CommandResult HttpDelete(string url, object content, HttpStatusCode? assertStatusCode = HttpStatusCode.OK, bool assertCommandOk = true)
        {
            var cr = ApiCall<CommandResult>(url, HttpMethod.Delete, content, assertStatusCode);

            return cr;
        }

        public CommandResultDto<TResult> HttpPostCR<TResult>(string url, object content, HttpStatusCode? assertStatusCode = HttpStatusCode.OK)
        {
            var r = ApiCall<CommandResultDto<TResult>>(url, HttpMethod.Post, content, assertStatusCode);

            return r;
        }

        public TResult HttpGetQR<TResult>(string url, HttpStatusCode? assertStatusCode = HttpStatusCode.OK, bool assertNotNull = true)
        {
            return HttpGetQ<TResult>(url, assertStatusCode, assertNotNull).Result;
        }

        public QueryResultDto<TResult> HttpGetQ<TResult>(string url, HttpStatusCode? assertStatusCode = HttpStatusCode.OK, bool assertNotNull = true)
        {
            var qr = ApiCall<QueryResultDto<TResult>>(url, HttpMethod.Get, null, assertStatusCode);

            Assert.That(!assertNotNull || qr.Result != null, $"Query result is null, url: {url}");

            return qr;
        }

        public TResult ApiCall<TResult>(
            string url,
            HttpMethod method,
            object content,
            HttpStatusCode? assertStatusCode = null,
            object urlParams = null)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new IEException("url is null");

            //if (urlParams != null)
            //{
            //    var props = urlParams.GetType().GetProperties();
            //    var urlParam = props.StringJoin("&"HttpUtility.UrlEncode());
            //}

            var msg = new HttpRequestMessage(method, url);

            if (content != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(content, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var result = RawApiCall(msg);

            Assert.True(
                !assertStatusCode.HasValue || (assertStatusCode.Value == result.StatusCode), 
                "assert status code failed. Expected {0}, current: {1}, url: {2}", assertStatusCode, result.StatusCode, url);

            var jsonResult = result.Content.ReadAsStringAsync().Result;
            var apiResult = JsonConvert.DeserializeObject<TResult>(jsonResult);

            return apiResult;
        }

        public HttpResponseMessage RawApiCall(HttpRequestMessage msg)
        {

            if (AuthType == AuthUserType.Admin)
            {
                msg.Headers.Add("Authorization", "Bearer " + JwtAdminToken);
            }
            else if (AuthType == AuthUserType.TestUser1)
            {
                msg.Headers.Add("Authorization", "Bearer " + JwtTestUser1Token);
            }
            else if (AuthType == AuthUserType.TestUser2)
            {
                msg.Headers.Add("Authorization", "Bearer " + JwtTestUser2Token);
            }

            var result = httpClient.SendAsync(msg).Result;

            return result;
        }   
    }
}
