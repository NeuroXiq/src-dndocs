using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DNDocs.Api.Client
{
    public static class DNDocsAppApiExtensions
    {
        public static void AddDNDocsApiClient(this WebApplicationBuilder builder, Action<DNDocsApiClientOptions> config = null)
        {
            var optionsBuilder = builder.Services.AddOptions<DNDocsApiClientOptions>().Bind(builder.Configuration.GetSection(nameof(DNDocsApiClientOptions)));

            if (config != null) optionsBuilder.Configure(config);

            optionsBuilder
                .Validate(o => !string.IsNullOrWhiteSpace(o.ServerUrl), $"{nameof(DNDocsApiClientOptions)}.ServerUrl (invalid option value)")
                .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), $"{nameof(DNDocsApiClientOptions)}.ServerUrl (invalid option value)")
                .ValidateOnStart();
            
            builder.Services.AddSingleton<IDNClient, DNClient>();
        }
    }
}
