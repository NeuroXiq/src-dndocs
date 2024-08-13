using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Job.Api.Client
{
    public static class DJobClientExtensions
    {
        public static void AddDJobClientFactory(this IServiceCollection services)
        {
            services.AddSingleton<IDJobClientFactory, DJobClientFactory>();
        }
    }
}
