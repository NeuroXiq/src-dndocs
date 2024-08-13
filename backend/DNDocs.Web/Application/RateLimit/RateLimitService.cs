using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DNDocs.Shared.Log;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace DNDocs.Web.Application.RateLimit
{
    public abstract class RateLimitHandler
    {
        public string Id { get; }
        public abstract RateLimitHandleResult Handle(RateLimitHandlerContext context);
        public abstract void Cleanup();

        public RateLimitHandler(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            Id = id;
        }
    }

    public struct RateLimitHandlerContext
    {
        public HttpContext HttpContext { get; set; }

        public RateLimitHandlerContext(HttpContext context)
        {
            HttpContext = context;
        }
    }

    public struct RateLimitHandleResult
    {
        public bool Allow;
        public RateLimitHandler Handler;
        public string Error;

        public RateLimitHandleResult(
            bool allowed,
            RateLimitHandler handler)
        {
            Allow = allowed;
            Handler = handler;
            Error = null;
        }
    }

    public class RLBox
    {
        public int Count;
        public DateTime LastUpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }

        public RLBox()
        {
            Reset();
        }

        public void Reset()
        {
            Count = 0;
            CreatedOn = DateTime.UtcNow;
            LastUpdatedOn = DateTime.UtcNow;
        }
    }

    public abstract class RateLimitHandlerBase : RateLimitHandler
    {
        public ConcurrentDictionary<string, RLBox> ratesInfo;
        private ILogger logger;

        protected Func<RateLimitHandlerContext, string> GetBoxId { get; set; }

        protected abstract bool ToClean(RLBox box);
        public abstract void DoHandle(RateLimitHandlerContext context,
            out bool allowed,
            out long xratelimit,
            out long xrateremaining,
            out long xratereset);

        public RateLimitHandlerBase(string id, Func<RateLimitHandlerContext, string> boxid, ILogger logger) : base(id)
        {
            this.logger = logger;
            this.GetBoxId = boxid;
            ratesInfo = new ConcurrentDictionary<string, RLBox>();
        }

        public override void Cleanup()
        {
            var entries = ratesInfo.ToArray();
            foreach (var e in entries)
            {
                var removed = ToClean(e.Value) && ratesInfo.Remove(e.Key, out _);
            }
        }

        public override RateLimitHandleResult Handle(RateLimitHandlerContext context)
        {
            DoHandle(context,
                out var allowed,
                out var xlimit,
                out var xrem,
                out var xreset);

            context.HttpContext.Response.Headers.Append("x-ratelimit-limit", xlimit.ToString());
            context.HttpContext.Response.Headers.Append("x-ratelimit-remaining", xrem.ToString());
            context.HttpContext.Response.Headers.Append("x-ratelimit-reset", xreset.ToString());

            var result = new RateLimitHandleResult(allowed, this);
            result.Error = $"Rate limit will reset at: {DateTimeOffset.FromUnixTimeSeconds(xreset).UtcDateTime} UTC";

            return result;
        }

        protected RLBox RLBoxGetOrCreate(RateLimitHandlerContext context)
        {
            var id = GetBoxId(context);
            ratesInfo.TryGetValue(id, out var box);
            
            if (box == null)
            {
                ratesInfo.TryAdd(id, new RLBox());
            }

            ratesInfo.TryGetValue(id, out var result);

            return result;
        }

        protected void RLBoxIncrement(RLBox box)
        {
            Interlocked.Increment(ref box.Count);
        }
    }

    public class RLStandardBoxId
    {
        public static readonly Func<RateLimitHandlerContext, string> ByHeaderAuthorization = (c) =>
            $"{c.HttpContext.Request.Path}<>{c.HttpContext.Request.Headers?.Authorization.ToString()}";

        public static readonly Func<RateLimitHandlerContext, string> ByIp = (c) =>
            $"{c.HttpContext.Request.Path}<>{c.HttpContext.Connection?.RemoteIpAddress?.ToString()}";
    }

    public class RateLimitHandlerFixedWindow : RateLimitHandlerBase
    {
        private Func<RateLimitHandlerContext, string> boxidFormat;
        
        TimeSpan windowTime;
        int maxCount;

        public RateLimitHandlerFixedWindow(
            string id,
            TimeSpan windowTime,
            int maxCount,
            Func<RateLimitHandlerContext, string> boxid,
            ILogger<RateLimitHandlerFixedWindow> logger) : base(id, boxid, logger)
        {
            this.windowTime = windowTime;
            this.maxCount = maxCount;
        }

        public override void DoHandle(RateLimitHandlerContext context,
            out bool allowed,
            out long xratelimit,
            out long xrateremaining,
            out long xratereset)
        {
            var box = RLBoxGetOrCreate(context);

            if (box.CreatedOn.Add(windowTime) < DateTime.UtcNow) box.Reset();

            allowed = box.Count < maxCount;
            xratelimit = maxCount;
            xrateremaining = maxCount - box.Count;
            xratereset = new DateTimeOffset(box.CreatedOn.Add(windowTime)).ToUnixTimeSeconds();

            if (allowed) base.RLBoxIncrement(box);
        }

        protected override bool ToClean(RLBox box)
        {
            return box.CreatedOn.Add(windowTime) < DateTime.UtcNow;
        }
    }

    public interface IRateLimitService
    {
        void Cleanup();
        RateLimitHandleResult Handle(HttpContext httpContext, RateLimitAttribute attributes);
    }

    public class RateLimitServiceOptions
    {
        public RateLimitHandler[] Handlers { get; set; }
    }

    public class RateLimitService : IRateLimitService
    {
        private RateLimitHandler[] handlers;
        private static ulong counter = 0;

        public RateLimitService(RateLimitHandler[] handlers)
        {
            if (handlers == null) throw new ArgumentNullException(nameof(handlers));
            var duplicatedId = handlers.Where(h => handlers.Count(a => a.Id == h.Id) > 1).FirstOrDefault();

            if (duplicatedId != null) throw new ArgumentException($"Handlers cannot have duplicated IDS, duplicated id: {duplicatedId.Id}");

            var allRLAttrs = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
                .SelectMany(t => t.GetMethods())
                .SelectMany(t => t.GetCustomAttributes<RateLimitAttribute>())
                .ToList();

            // validate all existsing attributes (for safety)
            foreach (var attr in allRLAttrs)
            {
                if (!handlers.Any(t => t.Id == attr.Id))
                    throw new InvalidOperationException($"There is declared RateLimitAttribute wiht id {attr.Id} but no handler for it");
            }

            this.handlers = handlers;
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public RateLimitHandleResult Handle(HttpContext httpContext, RateLimitAttribute attribute)
        {
            Interlocked.Increment(ref counter);

            if (counter % 1000 == 0)
            {
                foreach (var h in handlers) h.Cleanup();
            }

            var handlerToRun = handlers.Where(h => attribute.Id == h.Id).FirstOrDefault();

            if (handlerToRun == null)
            {
                throw new InvalidOperationException($"There is not handler for rate limit: {attribute.Id}");
            }

            var context = new RateLimitHandlerContext(httpContext);

            var result = handlerToRun.Handle(context);

            if (!result.Allow) return result;

            return new RateLimitHandleResult(true, null);
        }
    }
}
