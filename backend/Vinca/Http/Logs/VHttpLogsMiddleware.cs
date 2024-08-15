using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Utils;

namespace Vinca.Http.Logs
{
    internal class VHttpLogsMiddleware
    {
        private RequestDelegate next;
        private IVHttpLogService vhlService;
        private ILogger<VHttpLogsMiddleware> logger;

        public VHttpLogsMiddleware(
            IVHttpLogService vhlService,
            RequestDelegate next,
            ILogger<VHttpLogsMiddleware> logger)
        {
            this.next = next;
            this.vhlService = vhlService;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // todo object pool for timers & http
            var stopwatch = Stopwatch.StartNew();
            var start = DateTimeOffset.UtcNow;

            await next(context);
            
            var end = DateTimeOffset.UtcNow;
            stopwatch.Stop();

            if (vhlService.ShouldSaveLog(context))
            {
                var now = DateTime.UtcNow;
                var log = new VHttpLog()
                {
                    StartDate = start,
                    EndDate = end,
                    WriteLogDate = DateTimeOffset.UtcNow,
                    ClientIP = context.Connection.RemoteIpAddress.ToString(),
                    ClientPort = context.Connection.RemotePort,
                    Method = context.Request.Method,
                    UriPath = context.Request.Path,
                    UriQuery = context.Request.QueryString.ToString(),
                    ResponseStatus = context.Response.StatusCode,
                    BytesSend = context.Response.ContentLength,
                    BytesReceived = context.Request.ContentLength,
                    TimeTakenMs = stopwatch.ElapsedMilliseconds,
                    Host = context.Request.Headers.Host.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    Referer = context.Request.Headers.Referer.ToString()
                };

                vhlService.SaveLog(log);
            }
        }
    }
}
