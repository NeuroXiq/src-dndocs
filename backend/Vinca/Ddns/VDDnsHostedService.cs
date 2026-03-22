using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vinca.Ddns
{
    internal class VDDnsHostedService : IHostedService
    {
        private ILogger<VDDnsHostedService> logger;
        private IServiceProvider serviceProvider;
        private VDdnsHostedServiceOptions options;
        private CancellationTokenSource taskCancellationToken;
        private PeriodicTimer periodicTimer;
        private Task task;

        public VDDnsHostedService(ILogger<VDDnsHostedService> logger, IServiceProvider serviceProvider, IOptions<VDdnsHostedServiceOptions> options)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("starting service");
            taskCancellationToken = new CancellationTokenSource();
            periodicTimer = new PeriodicTimer(options.RefreshDdnsPeriod);
            task = DoWork();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("stopping service");

            if (periodicTimer != null)
            {
                periodicTimer.Dispose();
                periodicTimer = null;
            }

            taskCancellationToken.Cancel();

            await Task.WhenAny(Task.Delay(10000), task);
        }

        async Task DoWork()
        {
            logger.LogTrace(nameof(DoWork));

            while (await periodicTimer.WaitForNextTickAsync(taskCancellationToken.Token))
            {
                try
                {
                    using (var serviceScope = serviceProvider.CreateScope())
                    {
                        var ddnsService = serviceScope.ServiceProvider.GetRequiredService<IVDdnsService>();
                        await ddnsService.UpdateDdnsAsync(taskCancellationToken.Token);
                    }
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "failed to update ddns");
                }
            }
        }
    }
}
