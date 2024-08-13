using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Exceptions;

namespace Vinca.Api
{
    public interface IOSApi
    {
        /// <summary>
        /// suffix maybe useful to trace if not deleted or something goes wrong
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        IOSTempPath CreateTempDir(string fileNameSuffix = "vinca-dir-osapi");

        /// <summary>
        /// suffix maybe useful to trace if not deleted or something goes wrong
        /// </summary>
        /// <param name="suffix"></param>
        /// <returns></returns>
        IOSTempPath CreateTempFile(string fileNameSuffix = "vinca-file-osapi");

        Task ProcessStart(
            string psiFilename,
            string psiArguments,
            int waitTime,
            out int exitCode,
            out string stdo,
            out string stderr,
            bool throwIfExitCodeNotZero = true,
            string workingDirectory = null);
    }

    class OSApi : IOSApi
    {
        private ILogger<OSApi> logger;

        public OSApi(ILogger<OSApi> logger)
        {
            this.logger = logger;
        }

        public IOSTempPath CreateTempDir(string suffix = "vinca-dir-osapi")
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), $"-{suffix}");
            Directory.CreateDirectory(tempDirectory);

            return new OSTempPath(tempDirectory, OSTempPath.Type.Dir);
        }

        public IOSTempPath CreateTempFile(string fileNameSuffix = "vinca-file-osapi")
        {
            var filePath = Path.GetTempFileName();

            return new OSTempPath(filePath, OSTempPath.Type.File);
        }


        public Task ProcessStart(
            string psiFilename,
            string psiArguments,
            int waitTime,
            out int exitCode,
            out string stdo,
            out string stderr,
            bool throwIfExitCodeNotZero = true,
            string workingDirectory = null)
        {
            stdo = "";
            stderr = "";

            logger.LogTrace("Starting process: {0} {1} \r\nWait time:{2}\r\nWorking Directory: {3}", psiFilename, psiArguments, waitTime, workingDirectory);

            using (Process p = new Process())
            {
                p.StartInfo.FileName = psiFilename;
                p.StartInfo.Arguments = psiArguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WorkingDirectory = workingDirectory;
                p.Start();

                // p.PriorityClass = ProcessPriorityClass.RealTime; // not work on linux (permission denied)

                Thread.Sleep(5);
                int waiting = -1;

                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(1000 * waitTime);

                try
                {

                }
                catch (Exception e)
                {

                }

                do
                {
                    p.Refresh();
                    throw new NotImplementedException();
                    // p.WaitForExitAsync(token);
                    stdo += p.StandardOutput.ReadToEnd();
                    stderr += p.StandardError.ReadToEnd();
                }
                while (!p.HasExited && (waiting++) < waitTime);

                var plog = string.Format("PSI_FILENAME: {0}\r\nPSI_ARGS: {1} \r\nPID: {2}\r\nSTDO: {3}\r\nSTDERR: {4}\r\n", psiFilename, psiArguments, p.Id, stdo, stderr);

                VValidate.Throw(!p.HasExited, $"Process did not exit after wait time.\r\n{plog}");

                exitCode = p.ExitCode;

                plog = $"EXIT_CODE: {exitCode}\r\n" + plog;

                logger.LogTrace($"Process exe completed: {plog}");

                if (exitCode != 0) logger.LogWarning("process exit code not zero: \r\n {0}", plog);

                VValidate.AppEx(throwIfExitCodeNotZero && exitCode != 0, $"Process exit code not zero. {psiFilename} {psiArguments ?? "<NULL>"} PID: {p.Id} \r\n {plog}");
            }
        }
    }
}
