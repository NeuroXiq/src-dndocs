using DNDocs.Shared.Utils;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.Enum;
using System.Collections.Concurrent;
using System.Net;

namespace DNDocs.Web.Application.RateLimit
{
    public class RateLimitMiddleware
    {
        private ILogger<RateLimitMiddleware> logger;
        private IRateLimitService rateLimitService;
        private RequestDelegate next;

        public RateLimitMiddleware(
            RequestDelegate next,
            IRateLimitService rateLimitService,
            ILogger<RateLimitMiddleware> logger)
        {
            this.logger = logger;
            this.rateLimitService = rateLimitService;
            this.next = next;
        }

        public async Task<Task> InvokeAsync(HttpContext context)
        {
            var rateAttr = context?.GetEndpoint()?.Metadata?.GetMetadata<RateLimitAttribute>();
            var endpoint = context?.GetEndpoint();
            bool gonext = true;
            var result = default(RateLimitHandleResult);

            if (rateAttr != null)
            {
                result = rateLimitService.Handle(context, rateAttr);
                gonext = result.Allow;
            }

            if (gonext)
            {
                await next(context);
            }
            else
            {
                var logData = new string[]
                {
                    context.Request?.Path,
                    context.Request?.QueryString.ToString(),
                    context.Request?.Headers.StringJoin("\t\r\n", h => $"{h.Key}: {h.Value.StringJoin(",")}"),
                    context.Request?.Method,
                    context.Connection?.RemoteIpAddress?.ToString(),
                    "---------",
                    result.Error,
                    result.Handler?.GetType().FullName
                };

                logger.LogWarning("Rate Limit exceeded\r\n{0}", logData.StringJoin("\r\n"));
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsJsonAsync(new CommandResultDto(false, $"Rate limit exceeded. {result.Error}", null));
            }

            return Task.CompletedTask;
        }
    }
}
