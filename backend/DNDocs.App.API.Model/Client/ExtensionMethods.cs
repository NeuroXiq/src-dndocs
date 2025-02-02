using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.Client
{
    public static class ExtensionMethods
    {
        public static void AddDNClient(this IServiceCollection services, Action<DNClientOptions> config)
        {
            var dnoptions = new DNClientOptions();
            config?.Invoke(dnoptions);

            if (string.IsNullOrWhiteSpace(dnoptions.ApiKey)) throw new ArgumentException("apikey empty");
            if (string.IsNullOrWhiteSpace(dnoptions.ServerUrl)) throw new ArgumentException("serverurl empty");

            services.AddSingleton<IDNClient>(s => new DNClient(dnoptions));
        }
    }
}
