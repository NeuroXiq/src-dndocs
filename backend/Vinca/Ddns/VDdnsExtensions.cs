using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vinca.Ddns;

namespace Vinca.DDNS
{
    public static class VDdnsExtensions
    {
        const string ConfigSectionNameProkbunService = $"Vinca:{nameof(OptionsVDdnsPorkbunService)}";

        public static void AddVDdnsHostedService(this WebApplicationBuilder builder, Action<OptionsVDdnsHostedService> configure = null)
        {
            var optionsBuilder = builder.Services.AddOptions<OptionsVDdnsHostedService>();

            if (configure != null) optionsBuilder.Configure(configure);

            optionsBuilder
                .Validate(o => o.RefreshDdnsPeriod > TimeSpan.FromSeconds(30),
                    "(safety) RefreshDdnsPerion is very short less that 30 secods, and it will probably do api call every 30 seconds infinitely." +
                    "If this is intended remove this exception")
                .ValidateOnStart();

            builder.Services.AddHostedService<VDDnsHostedService>();
        }

        public static void AddVDdnsPorkbunService(this WebApplicationBuilder builder, Action<OptionsVDdnsPorkbunService> configure = null)
        {
            var optionsBuilder = builder.Services.AddOptions<OptionsVDdnsPorkbunService>().Bind(builder.Configuration.GetSection(ConfigSectionNameProkbunService));

            if (configure != null) optionsBuilder.Configure(configure);

            optionsBuilder
                .Validate(o => !string.IsNullOrWhiteSpace(o.SecretApiKey), "OptionsVDdnsProkbunService.ApiKeySecret")
                .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "OptionsVDdnsProkbunService.ApiKey")
                .Validate(o => !string.IsNullOrWhiteSpace(o.ApiUrl), "OptionsVDdnsProkbunService.ApiUrl")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Domain), "OptionsVDdnsProkbunService.Domain")
                .Validate(o => o.Subdomain == null || !string.IsNullOrWhiteSpace(o.Subdomain), "OptionsVDdnsProkbunService.Domain must be 'null' or valid subdomain (not white spaces etc.)")
                .ValidateOnStart();

            builder.Services.AddSingleton<IVDdnsService, VDdnsPorkbunService>();
        }
    }
}
