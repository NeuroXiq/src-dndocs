using Newtonsoft.Json;
using System.Diagnostics;

namespace DNDocs.IntegrationTests
{
    [SetUpFixture]
    internal class IntegrationTestsGlobalOneTimeSetup
    {
        private Process dndocsProcess;
        private Process dndocsDocsProcess;
        private Process dndocsJobProcess;

        public IntegrationTestsSettings Settings { get; private set; }
        public HttpClient HttpClientDNDocs { get; private set; }
        public HttpClient HttpClientDNDocsDocs { get; private set; }
        public HttpClient HttpClientDNDocsJob { get; private set; }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            HttpClientDNDocs?.Dispose();
            HttpClientDNDocsDocs?.Dispose();
            HttpClientDNDocsJob?.Dispose();

            if (Settings.EnvironmentName == "Development")
            {
                DevelopmentKillServerProcesses();
                //try { dndocsProcess?.Dispose(); dndocsProcess?.Kill(true); } catch { }
                //try { dndocsDocsProcess?.Dispose(); dndocsDocsProcess?.Kill(true); } catch { }
                //try { dndocsJobProcess?.Dispose(); dndocsJobProcess?.Kill(true); } catch { }
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Settings = System.Text.Json.JsonSerializer.Deserialize<IntegrationTestsSettings>(File.ReadAllText("./settings.json"));

            ValidateEnvironment();

            HttpClientDNDocs = new HttpClient();
            HttpClientDNDocsDocs = new HttpClient();
            HttpClientDNDocsJob = new HttpClient();

            HttpClientDNDocs.BaseAddress = new Uri(Settings.DNDocsServerUrl);
            HttpClientDNDocsDocs.BaseAddress = new Uri(Settings.DNDocsServerUrl);
            HttpClientDNDocsJob.BaseAddress = new Uri(Settings.DNDocsServerUrl);

            if (Settings.EnvironmentName == "Development")
            {
                SetUpDevelopmentEnvironment();
            }
        }

        private void ValidateEnvironment()
        {
            if (!new string[] { "Staging", "DevTest", "Development" }.Contains(Settings.EnvironmentName))
            {
                Assert.Fail("EnvironmentName from settings.json is not valid value. Valid values: Staging,DevTest,Development");
            }

            // if (!File.Exists(TestsAppConfig.PathSmallSizeZip))
            //     throw new Exception($"Startup exception: small path site  file does not exists in '{TestsAppConfig.PathSmallSizeZip}'");
            // 
            // if (!File.Exists(TestsAppConfig.PathBigSiteZip))
            //     throw new Exception($"Startup exception: big path site file does not exists in '{TestsAppConfig.PathBigSiteZip}'");



            if (Settings.EnvironmentName == "Development")
            {
                // when development:
                // remove current databases
                // start all three servers

                string backendDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../"));
                string dndocsSettings = Path.Combine(backendDir, "DNDocs.App.Web", "appsettings.Development.json");
                string dndocsDocsSettings = Path.Combine(backendDir, "DNDocs.Docs.Web", "appsettings.Development.json");
                string dndocsJobSettings = Path.Combine(backendDir, "DNDocs.Job.Web", "appsettings.Development.json");

                string dbDirDndocs = System.Text.Json.JsonDocument.Parse(File.ReadAllText(dndocsSettings))
                    .RootElement
                    .GetProperty("DNDocsSettings")
                    .GetProperty("DataDirectory")
                    .GetString();


                string dbDirDndocsDocs = System.Text.Json.JsonDocument.Parse(File.ReadAllText(dndocsDocsSettings))
                    .RootElement
                    .GetProperty("DOptions")
                    .GetProperty("DataDirectory")
                    .GetString();

                string dbDirDndocsJob = System.Text.Json.JsonDocument.Parse(File.ReadAllText(dndocsJobSettings))
                    .RootElement
                    .GetProperty("DJobSettings")
                    .GetProperty("DataDirectory")
                    .GetString();

                string[] allDbDirs = new string[] { dbDirDndocs, dbDirDndocsDocs, dbDirDndocsJob };

                if (!allDbDirs.All(Directory.Exists))
                {
                    Assert.Fail(
                        "development env: one or more directories does not exists. create them manually or debug exception reason.\n" +
                        string.Join(",", allDbDirs));
                }

                var allDbs = allDbDirs.SelectMany(dir => Directory.GetFiles(dir, "*.sqlite*")).ToArray();

                Console.WriteLine("found following databasese files to delete: \n {0}", string.Join("\n", allDbs));

                if (allDbs.Length > 30)
                    Assert.Fail("(safety reason) - failed to delete because looks like it is too many databases to delete(?)");

                foreach (var dbPath in allDbs)
                {
                    Console.WriteLine("delete: {0}", dbPath);
                    File.Delete(dbPath);
                }

                Console.WriteLine("completed deleting databases");
                Console.WriteLine("starting to run servers");

                var dndocsPsi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project {Path.Combine(backendDir, "DNDocs.App.Web/DNDocs.App.Web.csproj")}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                dndocsPsi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

                dndocsProcess = Process.Start(dndocsPsi);

                AppDomain.CurrentDomain.ProcessExit += (_, _) => { DevelopmentKillServerProcesses(); };
                AppDomain.CurrentDomain.DomainUnload += (_, _) => { DevelopmentKillServerProcesses(); };
                AppDomain.CurrentDomain.UnhandledException += (_, _) => { DevelopmentKillServerProcesses(); };

                // var process = 

                Debugger.Break();
            }
        }

        private void SetUpDevelopmentEnvironment()
        {
            throw new NotImplementedException();
        }

        private void DevelopmentKillServerProcesses()
        {
            var processes = new Process[] { dndocsDocsProcess, dndocsJobProcess, dndocsProcess };

            foreach (var p in processes)
            {
                try { dndocsProcess?.Kill(true); } catch { }
                try { dndocsProcess?.Dispose(); } catch { }
            }
        }
    }
}
