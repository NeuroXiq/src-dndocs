using DNDocs.Docs.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Docs.Api.Shared
{
    public static class Extensions
    {
        public static void AddDDocsApiClient(this IServiceCollection serviceCollection, Action<DDocsApiClientOptions> configure)
        {
            serviceCollection.Configure<DDocsApiClientOptions>(configure);
            var options = new DDocsApiClientOptions("", "");

            configure(options);
            
            // for safety only
            if (string.IsNullOrWhiteSpace(options.ServerUrl)) throw new ArgumentException("options.serverurl is empty");
            if (string.IsNullOrWhiteSpace(options.ApiKey)) throw new ArgumentException("options.apikey is empty");

            //serviceCollection.Configure<DDocsApiClientOptions>(o =>
            //{
            //    configure?.Invoke(options);
            //});

            serviceCollection.AddSingleton<IDDocsApiClient>(x => new DDocsApiClient(options));
        }
    }
}
