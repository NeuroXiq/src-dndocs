using DNDocs.Docs.Api.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DNDocs.Docs.Api.Shared
{
    public static class DNDocsDocsApiBuilderExtensions
    {
        public static void AddDNDocsDocsApiClient(this WebApplicationBuilder builder, Action<DNDocsDocsApiClientOptions> configure = null)
        {
            var optionsBuilder = builder.Services.AddOptions<DNDocsDocsApiClientOptions>().Bind(builder.Configuration.GetSection(nameof(DNDocsDocsApiClientOptions)));

            if (configure != null) optionsBuilder.Configure(configure);

            optionsBuilder
                .Validate(c => !string.IsNullOrWhiteSpace(c.ServerUrl), "DDocsApiClientOptions.ServerUrl")
                .Validate(c => !string.IsNullOrWhiteSpace(c.ApiKey), "DDocsApiClientOptions.ApiKey")
                .ValidateOnStart();

            builder.Services.AddSingleton<IDDocsApiClient, DDocsApiClient>();
        }
    }
}
