// using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNDocs.ConsoleTools
{
    //--testmigrations-deletedb
    // --run
    // 

    /// <summary>
    /// Not sure 100% but maybe this is not needed.
    /// Want to run docfx build in separate process (totally separate process as console app)
    /// because there are some problems with this. e.g. it generates 'templates' folder
    /// and probably not work well if multiple threads try to build this (even if everyting is 'created as temporary/transient'
    /// maybe some static stuff in docfx implementation? to investigate if console app is 100% needed.
    /// for safety run build always as totally separated process it will work as far as i tested this
    /// 
    /// </summary>
    // Probably remove this end remove this project from build process
    // leaving for future if something does not work
    // to have fallback if needed (now start docfx process as 'Process' directly from DJob

    public class Program
    {
        static int Main(string[] args)
        {
            //if (args.Length == 0) throw new Exception("no arguments");
            args = new string[] { "docfx", @"C:\Users\user\Desktop\docfx_project\docfx.json" };

            if (args[0]?.Trim() == "docfx")
            {
                return Docfx2(args[1]);
            }

            throw new Exception("unknown args");

            return -1;
        }

        private static int Docfx2(string docfxPath)
        {

            using var p = new Process();
            p.StartInfo.FileName = "docfx";
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(docfxPath);
            // p.StartInfo.Arguments = $@"build {osPathDocfxJson}";
            p.StartInfo.Arguments = @$"build docfx.json";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            // bool started = p.Start(

            var psi = new ProcessStartInfo("docfx", new string[] { docfxPath });
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.WorkingDirectory = Path.GetDirectoryName(docfxPath);

            var p2 = Process.Start(psi);

            p2.WaitForExit();

            var so = p2.StandardOutput.ReadToEnd();
            var se = p2.StandardError.ReadToEnd();
            var q = p2.ExitCode;
            Debugger.Break();
            return 0;
            //var t1 = Docfx.Dotnet.DotnetApiCatalog.GenerateManagedReferenceYamlFiles(docfxPath);
            //Task.WaitAll(t1);
            //var t2 = Docfx.Docset.Build(docfxPath);
            //Task.WaitAll(t2);
            //// Task.WaitAny(t);

            //t1.Wait();
            //t2.Wait();

            //return 0;
        }

        private static int Docfx_OLD_15082024(string[] args)
        {
            var docfxJsonPath = args[1];

            var tempLogger = new DocfxLogListener();

            var ao = new Microsoft.DocAsCode.BuildOptions
            {
                // now website is very unsafe because XSS are possible without any problems
                // need to find fix
                // ConfigureMarkdig = pipeline => pipeline.DisableHtml()
            };

            Microsoft.DocAsCode.Common.Logger.RegisterListener(tempLogger);

            var t2 = Microsoft.DocAsCode.Dotnet.DotnetApiCatalog.GenerateManagedReferenceYamlFiles(docfxJsonPath);
            var r = tempLogger.FormatString();
            //Debugger.Break();

            // t2.Wait();
            Task.WaitAny(t2);
            // if (t2.Exception != null) throw t2.Exception;


            tempLogger = new DocfxLogListener();
            Microsoft.DocAsCode.Common.Logger.RegisterListener(tempLogger);
            var t = Microsoft.DocAsCode.Docset.Build(docfxJsonPath, ao);

            t.Wait();

            var docfxBuildLogs = tempLogger.FormatString();

            if (tempLogger.LogItems.Any(l => l.LogLevel == Microsoft.DocAsCode.Common.LogLevel.Error))
            {
                return -1;
            }

            if (t.Exception != null)
            {
                return -1;
            }

            return 0;
        }

        class DocfxLogListener : Microsoft.DocAsCode.Common.ILoggerListener
        {
            List<string> logs = new List<string>();
            public List<ILogItem> LogItems = new List<ILogItem>();

            public void Dispose() { }
            public void Flush() { }

            public void WriteLine(ILogItem item)
            {
                LogItems.Add(item);
            }

            public string FormatString()
            {
                var sb = new StringBuilder();
                foreach (var item in LogItems)
                {
                    string l = "";
                    l += $"Code: {item.Code}\r\n";
                    l += $"File: {item.File}\r\n";
                    l += $"Line: {item.Line}\r\n";
                    l += $"LogLevel: {item.LogLevel}\r\n";
                    l += $"Message: {item.Message}\r\n";
                    l += $"Phase: {item.Phase}\r\n";
                    l += "---------------------------------------------";

                    sb.AppendLine(l);
                }

                return sb.ToString();
            }
        }
    }
}