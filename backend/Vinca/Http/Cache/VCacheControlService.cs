using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
    public interface IVCacheControlService
    {
        /// <summary>
        /// remember to implement 'false' when status code != 200 (e.g. status 500 return false)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool ShouldSkipCachingResponse(HttpContext context);
    }


    internal class VCacheControlService : IVCacheControlService
    {
        private Func<HttpContext, bool> callbackShouldSkipCachingResponse;
        private Func<HttpContext, bool> callbackShouldSkipCachingResponseAsync;

        public bool ShouldSkipCachingResponse(HttpContext context)
        {
            if (callbackShouldSkipCachingResponse != null) return callbackShouldSkipCachingResponse(context);

            return context.Response.StatusCode != 200;
        }
    }


    class CacheInfoEntry
    {
        public DateTime ExpiresAt { get; internal set; }
        public DateTime CreatedAt { get; set; }
        public string Etag { get; internal set; }

        public CacheInfoEntry(DateTime created, DateTime expiresAt, string etag)
        {
            CreatedAt = created;
            ExpiresAt = expiresAt;
            Etag = etag;
        }
    }
}
