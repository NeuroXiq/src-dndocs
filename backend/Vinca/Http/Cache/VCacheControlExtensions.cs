using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Cache
{
    public static class VCacheControlExtensions
    {
        /// <summary>
        /// Cache used between server and client without proxy between.
        /// Means client (e.g. webbrowser) will cache files locally (e.g. on user computer disk)
        /// </summary>
        /// <param name="builder"></param>
        public static void UseVCacheControl(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<VCacheControlMiddleware>();
        }

        public static void AddVCacheControlService(this IServiceCollection services, Action<VHttpCacheOptions> configure)
        {
            var options = new VHttpCacheOptions();
            configure?.Invoke(options);

            services.AddOptions<VHttpCacheOptions>();
            services.AddSingleton<IVCacheControlService, VCacheControlService>();
        }
    }

    public class VHttpCacheOptions
    {
        public Func<HttpContext, Task<CachedResourceInfo>> GetCacheResourceInfo;
    }
}
