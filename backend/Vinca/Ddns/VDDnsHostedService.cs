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
        private Task task;
        System.Threading.Timer timer;

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
            timer = new Timer(OnTimerCallback, null, 0, options.RefreshDdnsPeriod.Seconds);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("stopping service");
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            timer.Dispose();

            taskCancellationToken.Cancel();

            await Task.WhenAny(Task.Delay(10000), task);
        }

        void OnTimerCallback(object _)
        {
            logger.LogTrace(nameof(OnTimerCallback));
            taskCancellationToken.Cancel();
            taskCancellationToken = new CancellationTokenSource(120 * 60);
            task = DoWork();
        }

        async Task DoWork()
        {
            logger.LogTrace(nameof(DoWork));

            try
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    var ddnsService = serviceScope.ServiceProvider.GetRequiredService<IVDdnsService>();
                    await ddnsService.UpdateDdns(taskCancellationToken.Token);
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "failed to update ddns");
            }
        }
    }
}
