using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Cache
{
    public enum CacheAction
    {
        /// <summary>
        /// VHttp Private Cache middleware will not do anything,
        /// and invoke next delegate
        /// </summary>
        SkipMiddleware,
        SetHeadersAndContinue,
        NotModified304,
    }
}
