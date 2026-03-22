using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Vinca.Ddns;

namespace Vinca.DDNS
{
    public static class VDdnsExtensions
    {
        const string ConfigSectionNameProkbunService = $"Vinca:{nameof(VDdnsProkbunServiceOptions)}";

        public static void AddVDdnsHostedService(this WebApplicationBuilder builder, Action<VDdnsHostedServiceOptions> configure = null)
        {
            var optionsBuilder = builder.Services.AddOptions<VDdnsHostedServiceOptions>();

            if (configure != null) optionsBuilder.Configure(configure);

            optionsBuilder
                .Validate(o => o.RefreshDdnsPeriod > TimeSpan.FromSeconds(30),
                    "(safety) RefreshDdnsPerion is very short less that 30 secods, and it will probably do api call every 30 seconds infinitely." +
                    "If this is intended remove this exception")
                .ValidateOnStart();

            builder.Services.AddHostedService<VDDnsHostedService>();
        }

        public static void AddVDdnsPorkbunService(this WebApplicationBuilder builder, Action<VDdnsProkbunServiceOptions> configure = null)
        {
            var optionsBuilder = builder.Services.AddOptions<VDdnsProkbunServiceOptions>().Bind(builder.Configuration.GetSection(ConfigSectionNameProkbunService));

            if (configure != null) optionsBuilder.Configure(configure);

            builder.Services.AddSingleton<IVDdnsService, VDdnsPorkbunService>();
        }
    }
}
