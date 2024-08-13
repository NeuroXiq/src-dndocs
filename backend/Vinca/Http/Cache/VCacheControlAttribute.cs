using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Cache
{
    public enum CacheControlType
    {
        /// <summary>
        /// means that client (web browser) will  send http request to
        /// ask if content was modified. if content was not modified
        /// server responds with 304
        /// </summary>
        Public = 1,

        /// <summary>
        /// cache on client side (on disk) and no request
        /// will be send by webbrowser
        /// </summary>
        Private = 2
    }

    /// <summary>
    /// Options 'Cache-Control' http header
    /// </summary>
    public class VCacheControlAttribute : Attribute
    {
        public CacheControlType CacheType { get; set; }

        /// <summary>
        /// In seconds cache (http max-age header value)
        /// </summary>
        public int MaxAge { get; set; }
    }
}
