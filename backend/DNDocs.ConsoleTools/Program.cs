// using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


    public class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0) throw new Exception("no arguments");

            if (args[0]?.Trim() == "docfx")
            {
                return Docfx(args);
            }

            throw new Exception("unknown args");

            return -1;
        }

        private static int Docfx(string[] args)
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

            t2.Wait();
            if (t2.Exception != null) throw t2.Exception;


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