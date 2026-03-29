
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
            IHostApplicationLifetime applicationLifetime)
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
