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
        private static bool SetupReady = false;

        public IntegrationTestsSettings Settings { get; private set; }
        public HttpClient HttpClientDNDocs { get; private set; }
        public HttpClient HttpClientDNDocsDocs { get; private set; }
        public HttpClient HttpClientDNDocsJob { get; private set; }

        string serversPidsFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "to-stop-pid");

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            HttpClientDNDocs?.Dispose();
            HttpClientDNDocsDocs?.Dispose();
            HttpClientDNDocsJob?.Dispose();

            if (Settings.EnvironmentName == "Development")
            {
                TrickDevelopmentKillServerProcesses();
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (SetupReady) throw new Exception("something is wrong - this should be called only once");
            else SetupReady = true;

            Settings = System.Text.Json.JsonSerializer.Deserialize<IntegrationTestsSettings>(File.ReadAllText("./settings.json"));

            if (!new string[] { "Staging", "DevTest", "Development" }.Contains(Settings.EnvironmentName))
            {
                Assert.Fail("EnvironmentName from settings.json is not valid value. Valid values: Staging,DevTest,Development");
            }

            if (Settings.EnvironmentName == "Development")
            {
                SetUpDevelopmentEnvironment();
            }

            HttpClientDNDocs = new HttpClient();
            HttpClientDNDocsDocs = new HttpClient();
            HttpClientDNDocsJob = new HttpClient();

            HttpClientDNDocs.BaseAddress = new Uri(Settings.DNDocsServerUrl);
            HttpClientDNDocsDocs.BaseAddress = new Uri(Settings.DNDocsServerUrl);
            HttpClientDNDocsJob.BaseAddress = new Uri(Settings.DNDocsServerUrl);
        }

        private void ValidateEnvironment()
        {
            // if (!File.Exists(TestsAppConfig.PathSmallSizeZip))
            //     throw new Exception($"Startup exception: small path site  file does not exists in '{TestsAppConfig.PathSmallSizeZip}'");
            // 
            // if (!File.Exists(TestsAppConfig.PathBigSiteZip))
            //     throw new Exception($"Startup exception: big path site file does not exists in '{TestsAppConfig.PathBigSiteZip}'");
        }

        private void SetUpDevelopmentEnvironment()
        {
            // when development:
            // remove current databases
            // start all three servers

            TrickDevelopmentKillServerProcesses();

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

            TestContext.WriteLine("found following databasese files to delete: \n {0}", string.Join("\n", allDbs));

            if (allDbs.Length > 30)
                Assert.Fail("(safety reason) - failed to delete because looks like it is too many databases to delete(?)");

            var q = Process.GetProcesses().Where(t => t.ProcessName == "dotnet");
            var q2 = Process.GetProcesses();

            foreach (var dbPath in allDbs)
            {
                TestContext.WriteLine("delete: {0}", dbPath);
                File.Delete(dbPath);
            }

            TestContext.WriteLine("completed deleting databases");
            TestContext.WriteLine("starting to run servers");
            
            string[] paths = new string[] { "DNDocs.App.Web", "DNDocs.Job.Web", "DNDocs.Docs.Web" };

            foreach (var path in paths)
            {
                var dndocsPsi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project {Path.Combine(backendDir, $"{path}/{path}.csproj")}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = Path.Combine(backendDir, $"{path}/bin/Debug/net10.0")
                };

                dndocsPsi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

                Process p = new Process();
                p.StartInfo = dndocsPsi;

                p.OutputDataReceived += (s, e) => OnOutputReceived($"{path} STDO", e.Data);
                p.ErrorDataReceived += (s, e) => OnOutputReceived($"{path} STDERR", e.Data);
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                File.AppendAllText(serversPidsFile, " " + p.Id.ToString());
            }

            AppDomain.CurrentDomain.ProcessExit += (_, _) => { TrickDevelopmentKillServerProcesses(); };
            AppDomain.CurrentDomain.DomainUnload += (_, _) => { TrickDevelopmentKillServerProcesses(); };
            AppDomain.CurrentDomain.UnhandledException += (_, _) => { TrickDevelopmentKillServerProcesses(); };

            Thread.Sleep(2000);

            // wait max 20s to servers go up
            HttpClient testReadyClient = new HttpClient();
            bool ready = false;

            for (int i = 0; !ready && i < 20; i++)
            {
                Thread.Sleep(1000);
                try
                {
                    testReadyClient.GetStringAsync(Settings.DNDocsServerUrl + "/api/system/health").Wait();
                    testReadyClient.GetStringAsync(Settings.DNDocsJobServerUrl + "/api/system/health").Wait();
                    testReadyClient.GetStringAsync(Settings.DNDocsDocsServerUrl + "/api/system/health").Wait();
                    ready = true;
                }
                catch (Exception e)
                {
                    Debugger.Break();
                }
            }

            if (!ready)
            {
                throw new Exception("servers not ready after 20s. Try again or debug reason");
            }
        }

        static string TempOutput = "";

        private void OnOutputReceived(string logName, string output)
        {
            // can redirect output if want to debug what happening/not starting
            // or can uncomment static string var to see all logs

            // TempOutput += output;
            // File.AppendAllText(@"C:\my-files\projects\itout.txt", output + "\r\n");
        }

        private void TrickDevelopmentKillServerProcesses()
        {
            if (!File.Exists(serversPidsFile))
            {
                File.WriteAllText(serversPidsFile, " ");
            }

            var processesIds = File.ReadAllText(serversPidsFile)?.Trim()?.Split(' ') ?? new string[0];

            foreach (string pid in processesIds)
            {
                try { Process.GetProcessById(int.Parse(pid))?.Kill(true); } catch { }
            }

            var processes = new Process[] { dndocsDocsProcess, dndocsJobProcess, dndocsProcess };
        }
    }
}
