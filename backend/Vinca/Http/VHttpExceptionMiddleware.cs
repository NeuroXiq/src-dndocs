using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Exceptions;

namespace Vinca.Http
{
    internal class VHttpExceptionMiddleware
    {
        private RequestDelegate next;
        private ILogger<VHttpExceptionMiddleware> logger;

        public VHttpExceptionMiddleware(
            RequestDelegate next,
            ILogger<VHttpExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (VHttpException e)
            {
                logger.LogError(e, "VHttpException");
                context.Response.ContentType = "application/json; charset=UTF-8";
                context.Response.StatusCode = (int)e.StatusCode;
                await context.Response.WriteAsJsonAsync(new { Error = e.Message });
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "ise");
                context.Response.StatusCode = 500;
            }
        }
    }
}
