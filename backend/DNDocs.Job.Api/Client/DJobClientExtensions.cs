using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DNDocs.Job.Api.Client
{
    public static class DJobClientExtensions
    {
        public static void AddDNDocsJobApiClient(this WebApplicationBuilder builder, Action<DNDocsJobApiClientOptions> configure = null)
        {
            var optionsBuilder = builder.Services.AddOptions<DNDocsJobApiClientOptions>().Bind(builder.Configuration.GetSection(nameof(DNDocsJobApiClientOptions)));

            if (configure != null) optionsBuilder.Configure(configure);

            optionsBuilder
                .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "DNDocsJobClientOptions.ApiKey")
                .Validate(o => !string.IsNullOrWhiteSpace(o.ServerUrl), "DNDocsJobClientOptions.ServerUrl")
                .ValidateOnStart();

            builder.Services.AddScoped<IDNDocsJobApiClient, DNDocsJobApiClient>();
        }
    }
}
