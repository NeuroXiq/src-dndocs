
using DNDocs.Api.Client;
using DNDocs.Api.Model.Integration;
using DNDocs.Job.Web.Services;
using DNDocs.Job.Web.Shared;
using Microsoft.Extensions.Options;
using Vinca.BufferLogger;
using Vinca.Exceptions;

namespace DNDocs.Job.Web.Application
{
    public class DJobHostedService : IHostedService
    {
        private DJobSettings dsettings;
        private IDNClient dnclient;
        private ILogger<DJobHostedService> logger;
        private IDJobRepository repository;
        private IVBufferLogger bufferLogger;
        private IBgJobsService bgjobService;
        private IHostApplicationLifetime applicationLifetime;
        private Timer logsTimer;
        private Task saveLogsTask = Task.CompletedTask;

        public DJobHostedService(
            IDJobRepository repository,
            IVBufferLogger bufferLogger,
            IBgJobsService bgjobService,
            ILogger<DJobHostedService> logger,
            IDNClient dnclient,
            IOptions<DJobSettings> dsettings,
            IHostApplicationLifetime applicationLifetime
            )
        {
            this.dsettings = dsettings.Value;
            this.dnclient = dnclient;
            this.logger = logger;
            this.repository = repository;
            this.bufferLogger = bufferLogger;
            this.bgjobService = bgjobService;
            this.applicationLifetime = applicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logsTimer = new Timer(LogsTimerCallback, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            await bgjobService.AppStart();


            // this is important to run after application started important for dev 
            // because DN sends ping immediately even before DJob for HTTP requests
            applicationLifetime.ApplicationStarted.Register(() => RegisterServiceOnDNDocs());
        }

        private void RegisterServiceOnDNDocs()
        {
            Task.Factory.StartNew(() =>
            {
                bool success = false;
                for (int i = 0; i < 3 && !success; i++)
                {
                    logger.LogTrace("start register on DNDocs {0}", i);

                    try
                    {
                        var result = dnclient.Integration_DJobRegisterService(new DJobRegisterServiceModel
                        {
                            InstanceName = $"DJOB-INSTANCE-{Guid.NewGuid()}",
                            ServerPort = this.dsettings.KestrelPort
                        }).Result;

                        success = result.Success;
                        if (!success) logger.LogCritical("startup sending to DN returned failure (success=false). error: {0}", result.ErrorMessage);
                        else logger.LogInformation("startup DN register success");
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical(e, "failed to register instance on DNDocs");
                        Thread.Sleep(3000);
                        throw;
                    }
                }
            });
        }

        private void LogsTimerCallback(object state)
        {
            if (bufferLogger.IsEmpty) return;
            var logs = bufferLogger.DequeueAllLogs();
            if (logs.Count == 0) return;

            saveLogsTask = Task.Factory.StartNew(() => { repository.InsertLogs(logs); });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await bgjobService.AppStopAsync();
        }
    }
}
