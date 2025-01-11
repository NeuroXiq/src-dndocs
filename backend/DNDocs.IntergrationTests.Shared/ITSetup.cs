
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DNDocs.IntergrationTests.Shared
{
    public class ITSetup
    {
        static Process DDocsProcess;

        public static void StartServer_DJob() { }

        public static void StartServer_DDocs()
        {
            TrickKillExistingProcess(Path.GetFileNameWithoutExtension(TestsAppConfig.PathDDocsWebExe));
            CleanupInfrastructureFiles(TestsAppConfig.PathDDocsTestsInfrastructureDir);
            DDocsProcess = StartServer(TestsAppConfig.PathDDocsWebExe);
        }

        public static void StartServer_DN()
        {
        }

        static void CleanupInfrastructureFiles(string directory)
        {
            var itpath = directory;
            if (!(itpath?.EndsWith(@"\var\it-ddocs") == true))
            {
                throw new Exception("'!(itpath?.EndsWith(@\"\\var\\it-ddocs\") == true)': is this corrent? throwing for safe purpose before delete");
            }

            var files = Directory.GetFiles(directory).ToList();
            files.ForEach(File.Delete);
        }

        static void TrickKillExistingProcess(string processName)
        {
            var runningProcesses = Process.GetProcessesByName("DNDocs.Docs.Web");
            if (runningProcesses.Any())
            {
                // todo: investigate, sometimes in tests debug process is still running
                // (probably global TearDown not run when killing/stopping process from VS)
                // so kill manually
                runningProcesses.FirstOrDefault()?.Kill();
                // wait second for OS kill process
                Thread.Sleep(1000);
            }
        }

        static Process StartServer(string webDllFilePath)
        {
            string processName = Path.GetFileNameWithoutExtension(webDllFilePath);
            string workingDirectory = Path.GetDirectoryName(webDllFilePath);
            
            // need to start real server to perform http requests on real environment
            // now starting with debug environment

            var existing = Process.GetProcessesByName(processName);
            if (existing.Length == 1) { existing[0].Kill(); }
            else if (existing.Length > 1) throw new Exception($"startup exception, more than 1 process {processName} found. unexpected");
            var startInfo = new ProcessStartInfo();

            // false -> want to this process be child of current proceess
            // to be destroyd after tests ends
            startInfo.UseShellExecute = false;

            startInfo.FileName = webDllFilePath;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.EnvironmentVariables.Add("ASPNETCORE_ENVIRONMENT", "IntegrationTests");
            startInfo.WorkingDirectory = workingDirectory;

            Process serverProcess = Process.Start(startInfo);

            Thread.Sleep(500);

            // need to wait a second because 
            // sometimes tests start before app setup finish
            // question: when to know when app is ready to use?

            //bool ok = false;

            //var clientCheckAlive = TestsBase.CreateNewHttpClient();
            //for (int i = 0; i < 10; i++)
            //{
            //    try
            //    {
            //        Thread.Sleep(500);
            //        clientCheckAlive.Public_Ping();
            //        ok = true;
            //    }
            //    catch { }
            //}

            //if (!ok) throw new Exception("failed to ping or run ddocs.docs server after 5 seconds");

            //if (p.HasExited) throw new Exception("dndocs.docs failed to start, exited right after start");
            ObserveDndocsProcess(serverProcess);
            Thread.Sleep(2000);

            return serverProcess;
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
