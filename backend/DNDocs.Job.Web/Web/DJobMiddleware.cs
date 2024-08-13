using Microsoft.Extensions.Options;
using System.Diagnostics;
using Vinca.Exceptions;

namespace DNDocs.Job.Web.Web
{
    public class DJobMiddleware
    {
        private RequestDelegate next;
        private ILogger<DJobMiddleware> logger;

        public DJobMiddleware(
           RequestDelegate next,
           ILogger<DJobMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var r = context.Request;
            try
            {
                await next(context);
            }
            catch (VStatusCodeException e)
            {
                logger.LogWarning(e, "status code exception");

                context.Response.StatusCode = e.StatusCode;
                await context.Response.WriteAsync(e.Message);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                logger.LogCritical(e, "ISE");
            }
            sw.Stop();

            logger.LogInformation("HTTP: {0} {1} {2} {3}:{4} {5} {6}ms",
                r.Method,
                r.Path.ToString(),
                r.QueryString.ToString(),
                context.Connection.RemoteIpAddress?.ToString(),
                context.Connection.RemotePort,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }
}
