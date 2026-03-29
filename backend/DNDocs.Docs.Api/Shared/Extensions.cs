using DNDocs.Docs.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DNDocs.Docs.Api.Shared
{
    public static class Extensions
    {
        public static void AddDNDocsDocsApiClient(this IServiceCollection serviceCollection, Action<DNDocsDocsApiClientOptions> configure = null)
        {
            var optionsBuilder = serviceCollection.AddOptions<DNDocsDocsApiClientOptions>();

            if (configure != null)
            {
                optionsBuilder.Configure(configure);
            }

            optionsBuilder
                .Validate(c => !string.IsNullOrWhiteSpace(c.ServerUrl), "DDocsApiClientOptions.ServerUrl")
                .Validate(c => !string.IsNullOrWhiteSpace(c.ApiKey), "DDocsApiClientOptions.ApiKey")
                .ValidateOnStart();

            serviceCollection.AddSingleton<IDDocsApiClient, DDocsApiClient>();
        }
    }
}
