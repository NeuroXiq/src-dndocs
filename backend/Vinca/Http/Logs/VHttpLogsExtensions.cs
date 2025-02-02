using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Logs
{
   public static class VHttpLogsExtensions
   {
        public static void UseVHttpLogs(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<VHttpLogsMiddleware>();
        }

        public static void AddVHttpLogs(this IServiceCollection services, Action<VHttpLogOptions> configue)
        {
            services.Configure<VHttpLogOptions>((opt) =>
            {
                opt.MaxQueueSize = 2000;
                opt.ShouldSaveLog = null;
                configue?.Invoke(opt);
            });

            services.AddSingleton<IVHttpLogService, VHttpLogService>();

            //services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            //services.TryAddSingleton<ObjectPool<Stopwatch>>(serviceProvider =>
            //{
            //    var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            //    var policy = new StopwatchPolicy();

            //    return provider.Create(policy);
            //});
        }


        class StopwatchPolicy : PooledObjectPolicy<Stopwatch>
        {
            public override Stopwatch Create()
            {
                return new Stopwatch();
            }

            public override bool Return(Stopwatch obj)
            {
                obj.Reset();
                return true;
            }
        }
    }
}
