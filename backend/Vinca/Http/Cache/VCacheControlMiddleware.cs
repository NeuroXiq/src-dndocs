using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Cache
{
    //SomeDate.ToUniversalTime().ToString("r");
    internal class VCacheControlMiddleware
    {
        private RequestDelegate next;
        private IVCacheControlService ccservice;
        private static ConcurrentDictionary<string, CacheInfoEntry> cacheEntriesDict = new ConcurrentDictionary<string, CacheInfoEntry>();
        private Func<HttpContext, Task<CachedResourceInfo>> getResourceInfo;

        public VCacheControlMiddleware(
            RequestDelegate next,
            IVCacheControlService ccservice)
        {
            this.next = next;
            this.ccservice = ccservice;
            this.getResourceInfo = null;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // todo this needs tests:
            // bool is304NotModifiedUsed = await TryUseNotModified304(context);

            // logger.LogTrace("will use 304 content not modified: {0}, for path: {1}", is304NotModifiedUsed, context.Request.Path);
            // 
            // if (is304NotModifiedUsed) return;
            
            // need to set http-headers before writing reponse
            context.Response.OnStarting(OnResponseStarting, context);

            await next(context);
        }

        private bool ShouldSkipCachingResponse(HttpContext context)
        {
            return context.Response.StatusCode != 200;
        }


        private async Task OnResponseStarting(object state)
        {
            var context = (HttpContext)state;

            if (ccservice.ShouldSkipCachingResponse(context)) return;

            var cachePolicy = GetCachePolicy(context);
            if (cachePolicy == null) return;

            bool statusCodeAllowsCache = context.Response.StatusCode == (int)HttpStatusCode.OK;
            bool addCurrentResultToCache = statusCodeAllowsCache;
            var cacheInfoKey = GetCacheInfoKey(context);
            CacheInfoEntry cacheEntry;
            CachedResourceInfo resourceInfo = (getResourceInfo == null ? null : await getResourceInfo(context));

            if (addCurrentResultToCache)
            {
                
                // we didnt cache anything, create new entry
                if (!cacheEntriesDict.TryGetValue(cacheInfoKey, out cacheEntry))
                {
                    var newEntry = new CacheInfoEntry(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(cachePolicy.MaxAge), resourceInfo?.FileTag);
                    cacheEntriesDict.TryAdd(cacheInfoKey, newEntry);
                }

                // mdn: If there is a Cache-Control header with the max-age or s-maxage directive in the response, the Expires header is ignored.
                SetHeaders(context, cachePolicy.CacheType, cachePolicy.MaxAge, null, resourceInfo?.FileTag, null);
            }
        }

        VCacheControlAttribute GetCachePolicy(HttpContext context)
        {
            return context.GetEndpoint()?.Metadata.GetMetadata<VCacheControlAttribute>();
        }

        void SetHeaders(HttpContext context, CacheControlType cacheType, int? maxAge, int? age, string etag, DateTime? expires)
        {
            var cacheControl = cacheType == CacheControlType.Private ? "private" : "public";
            if (maxAge.HasValue) cacheControl += $",max-age={maxAge.Value}";

            context.Response.Headers.Append("Cache-Control", new StringValues(cacheControl));
            if (age.HasValue) context.Response.Headers.Append("Age", new StringValues(age.Value.ToString()));
            if (etag != null) context.Response.Headers.Append("Etag", new StringValues(etag));
            if (expires.HasValue) context.Response.Headers.Append("Expires", expires.Value.ToUniversalTime().ToString("r"));
        }

        public async Task<bool> TryUseNotModified304(HttpContext context)
        {
            // todo before this code add 'get custom external action from configuration of this service'
            // because users may want custom behaviour
            // Cache-Control: public, max-age=31536000, stale-while-revalidate=2592000
            var cachePolicy = GetCachePolicy(context);
            if (cachePolicy == null) return false;


            var ifModSinceHeader = context.Request.Headers.IfModifiedSince;
            var ifNonMatchHeader = context.Request.Headers.IfNoneMatch;
            var varyHeader = context.Request.Headers.Vary;
            var pragmaHeader = context.Request.Headers.Pragma;
            var cacheControlHeader = context.Request.Headers.CacheControl;

            DateTime? ifModSince = ifModSinceHeader.Count == 0 ? null : DateTime.ParseExact(ifModSinceHeader[0], "R", CultureInfo.InvariantCulture);
            string ifNonMatch = ifNonMatchHeader.Count == 0 ? null : ifNonMatchHeader[0];
            string[] vary = varyHeader.ToArray();
            string[] pragma = pragmaHeader.ToArray();
            string[] cacheControl = cacheControlHeader.ToArray();

            var cacheInfoKey = GetCacheInfoKey(context);
            CacheInfoEntry cacheEntry;
            CachedResourceInfo cachedResourceInfo = null;

            // remove old cache
            if (cacheEntriesDict.TryGetValue(cacheInfoKey, out cacheEntry))
            {
                if (DateTime.UtcNow > cacheEntry.ExpiresAt)
                {
                    // todo is this read save? what if other thread right before modified this?
                    cacheEntriesDict.TryRemove(cacheInfoKey, out _);
                    cacheEntry = null;
                }
            }

            if (cachePolicy.CacheType == CacheControlType.Public && getResourceInfo != null)
            {
                cachedResourceInfo = await getResourceInfo(context);
            }

            bool resourceInfoAllowsUseCached = true;
            if (cachedResourceInfo != null)
            {
                resourceInfoAllowsUseCached &= cachedResourceInfo.FileTag == null || ifNonMatchHeader.FirstOrDefault() != cachedResourceInfo.FileTag;
                resourceInfoAllowsUseCached &= !ifModSince.HasValue || cachedResourceInfo.LastModifiedAt <= ifModSince.Value;
                // todo cachepolicy.maxage check?
            }

            bool userWantCache = // cacheControlHeader.Any(t => string.Compare(t, "no-cache", StringComparison.OrdinalIgnoreCase) == 0)
                 ifModSince.HasValue || ifNonMatchHeader.Count != 0;

            bool canUse304NotModified = resourceInfoAllowsUseCached && userWantCache && cacheEntry != null;

            if (canUse304NotModified)
            {
                SetHeaders(
                    context,
                    cachePolicy.CacheType,
                    null,
                    (int)(DateTime.UtcNow - cacheEntry.CreatedAt).TotalSeconds,
                    cachedResourceInfo?.FileTag,
                    cacheEntry.CreatedAt.AddSeconds(cachePolicy.MaxAge));

                context.Response.StatusCode = (int)HttpStatusCode.NotModified;

                return true;
            }

            return false;
        }


        private string GetCacheInfoKey(HttpContext context)
        {
            return context.Request.Path;
        }
    }
}
