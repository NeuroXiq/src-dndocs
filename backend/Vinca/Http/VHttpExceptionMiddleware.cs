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
            catch (VValidationException e)
            {
                logger.LogError("Validation error: {0} ", e.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                context.Response.ContentType = "application/json; charset=UTF-8";
                await context.Response.WriteAsJsonAsync(new { ValidationError = e.Message });
            }
            catch (VStatusCodeException e)
            {
                context.Response.StatusCode = (int)e.StatusCode;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "ISE");
            }
        }
    }
}
