using Microsoft.Extensions.DependencyInjection;
using Vinca.Ddns;

namespace Vinca.DDNS
{
    public static class VDdnsExtensions
    {
        public static void AddVDdnsHostedService(this IServiceCollection services, Action<VDdnsHostedServiceOptions> configure)
        {
            var optionsBuilder = services.AddOptions<VDdnsHostedServiceOptions>();

            if (configure != null)
            {
                optionsBuilder.Configure(configure);
            }

            optionsBuilder
                .Validate(o => o.RefreshDdnsPeriod > TimeSpan.FromSeconds(30),
                    "(safety) RefreshDdnsPerion is very short less that 30 secods, and it will probably do api call every 30 seconds infinitely." +
                    "If this is intended remove this exception")
                .ValidateOnStart();

            services.AddHostedService<VDDnsHostedService>();
        }

        public static void AddVDdnsPorkbunService(this IServiceCollection services, Action<VDdnsProkbunServiceOptions> configure)
        {
            var optionsBuilder = services.AddOptions<VDdnsProkbunServiceOptions>();

            if (configure != null)
            {
                optionsBuilder.Configure(configure);
            }

            services.AddSingleton<IVDdnsService, VDdnsPorkbunService>();
        }
    }
}
