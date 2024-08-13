using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Vinca.Exceptions;
using Vinca.Utils;

namespace Vinca.Http
{
    public class XApiKey
    {
        public static void Validate(HttpContext context, string apiKey, ILogger logger)
        {
            Exception exc = null;

            try
            {
                if (context.Request.Headers.TryGetValue("x-api-key", out var apiKeyHeader))
                {
                    if (apiKeyHeader.Count == 1 && apiKeyHeader[0] == apiKey)
                    {
                        return;
                    }
                }

                logger.LogWarning("unauthorized,  path: {0}, remote ip: {1}, remote port: {2}, received x-api-key in header: '{3}' headers:\r\n{4} ",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress.ToString(),
                    context.Connection.RemotePort,
                    apiKeyHeader.Count > 0 ? apiKeyHeader.ToString() : "",
                    context.Request.Headers.Select(t => $"{t.Key}: {t.Value.StringJoin(", ")}\r\n"));

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            catch (Exception e)
            {
                exc = e;
            }

            throw new VStatusCodeException(HttpStatusCode.Unauthorized, "unauthorized");
        }
    }
}
