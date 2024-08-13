using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Cache
{
    public class CacheActionData
    {
        public CacheAction Action;
        public CacheControlType CacheType;
        public int? CCMaxAge;
        //.ToUniversalTime().ToString("r");
        public string Etag;
        public DateTime? Expires;

        public int? Age { get; internal set; }

        // public DateTime? LastModified;


        public static CacheActionData SkipCacheMiddleware()
        {
            return new CacheActionData { Action = CacheAction.SkipMiddleware };
        }
    }
}
