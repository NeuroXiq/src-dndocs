using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http
{
    public class HttpClientHandlerLogger : HttpClientHandler
    {
        private ILogger logger;

        public HttpClientHandlerLogger(ILogger logger)
        {
            this.logger = logger;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return LoggerWrap(false, request, cancellationToken).Result;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await LoggerWrap(true, request, cancellationToken);
        }

        private async Task<HttpResponseMessage> LoggerWrap(bool asAsync, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            
            logger.LogTrace("HTTP: Starting to send {0} {1} {2}", DateTime.UtcNow, request.Method, request.RequestUri);
            try
            {
                logger.LogTrace("HTTP starting to send {0} {1} {2}", DateTime.UtcNow, request.Method, request.RequestUri);

                HttpResponseMessage response = null;

                if (asAsync) response = await base.SendAsync(request, cancellationToken);
                else response = base.Send(request, cancellationToken);

                sw.Stop();
                logger.LogTrace("HTTP completed request: {0} {1} {2} duration: {3}ms | response: {4}",
                    DateTime.UtcNow,
                    request.Method,
                    request.RequestUri,
                    sw.ElapsedMilliseconds,
                    response.StatusCode);

                return response;
            }
            catch (Exception e)
            {
                logger.LogError(e, "HTTP exception during request {0} {1} {2}", DateTime.UtcNow, request.Method, request.RequestUri);
                throw;
            }
        }
    }
}
