using DNDocs.Application.CommandHandlers.Application;
using DNDocs.Application.Commands.Application;
using DNDocs.Application.Services;
using DNDocs.Application.Shared;
using DNDocs.Docs.Api.Client;
using DNDocs.Docs.Api.Management;
using DNDocs.Domain.Entity;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Infrastructure.Utils;
using DNDocs.Shared.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using Vinca.Api;
using Vinca.BufferLogger;
using Vinca.Http.Logs;
using Vinca.Utils;

namespace DNDocs.Application.Application
{
    public class ApiBackgroundWorker : IHostedService, IDisposable
    {
        static object _lock = new object();
        CancellationTokenSource cancellationTokenSource;
        private IBgJobQueue bgjobQueue;
        private readonly int SleepSecondsDoImportantWork;
        private readonly int SleepSecondsDoWork;
        private Timer timerSaveLogs;
        private Timer timerBuildProjects;
        private Timer timerIndexNow;
        private Timer timerProcessNugetCatalog;
        private IWebHostEnvironment webHostEnvironment;
        private IIndexNowApi indexNowApi;
        private IDDocsApiClient ddocsApiClient;
        private IVHttpLogService vHttpLogs;
        private IDNInfrastructure dinfrastructure;
        private IVBufferLogger ivBufferLogger;
        private IServiceProvider services;
        private ILogger<ApiBackgroundWorker> logger;
        private Task taskIndexNow = Task.CompletedTask;
        private Task taskBuildProjects = Task.CompletedTask;
        private Task taskProcessNuGetCatalog = Task.CompletedTask;

        public ApiBackgroundWorker(IServiceProvider services,
            ILogger<ApiBackgroundWorker> logger,
            IOptions<DNDocsSettings> robiniaSettings,
            IBgJobQueue bgjobQueue,
            IVBufferLogger ivBufferLogger,
            IDNInfrastructure dinfrastructure,
            IVHttpLogService vHttpLogs,
            IDDocsApiClient ddocsApiClient,
            IIndexNowApi indexNowApi,
            IWebHostEnvironment webHostEnvironment
            )
        {
            this.webHostEnvironment = webHostEnvironment;
            this.indexNowApi = indexNowApi;
            this.ddocsApiClient = ddocsApiClient;
            this.vHttpLogs = vHttpLogs;
            this.dinfrastructure = dinfrastructure;
            this.ivBufferLogger = ivBufferLogger;
            this.services = services;
            this.logger = logger;
            this.SleepSecondsDoImportantWork = robiniaSettings.Value.BackendBackgroundWorkerDoImportantWorkSleepSeconds;
            this.SleepSecondsDoWork = robiniaSettings.Value.BackendBackgroundWorkerDoWorkSleepSeconds;
            cancellationTokenSource = new CancellationTokenSource();
            this.bgjobQueue = bgjobQueue;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting backend background service");

            timerSaveLogs = new Timer(BgWork_SaveLogs, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(SleepSecondsDoImportantWork));
            timerBuildProjects = new Timer(BgWork_BuildProjects, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(SleepSecondsDoWork));
            timerIndexNow = new Timer(OnTimerIndexNow, null, TimeSpan.FromSeconds(5), TimeSpan.FromHours(24.1));
            timerProcessNugetCatalog = new Timer(OnTimerProcessNugetCatalog, null, TimeSpan.FromSeconds(5), TimeSpan.FromHours(24));

            await bgjobQueue.OnSystemStart();

            // 1. generate projects
            // 2. cleanup bgjob remote services
        }

        private void OnTimerProcessNugetCatalog(object _)
        {
            lock (_lock)
            {
                if (!taskProcessNuGetCatalog.IsCompleted) return;
                taskProcessNuGetCatalog = Task.Run(() => RunCommand(new BgJobProcessNugetCatalogCommand()));
            }
        }

        private void OnTimerIndexNow(object state)
        {
            if (!taskIndexNow.IsCompleted) return;
            taskIndexNow = Task.Run(DoIndexNow);
        }

        private async Task DoIndexNow()
        {
            if (webHostEnvironment.IsDevelopment()) return;

            logger.LogTrace("starting doindexnow");

            using var scope = services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
            var indexNowRepository = uow.GetSimpleRepository<IndexNowLog>();

            List<SiteItemDto> siteItems = new List<SiteItemDto>();
            bool lastAnyUrls = true;
            int counter = 0;
            long nextStartId = indexNowRepository.Query().Any() ?
                indexNowRepository.Query().Max(t => t.SiteItemIdEnd) + 1
                : 1;

            do
            {
                await Task.Delay(1000);

                IList<SiteItemDto> items = await ddocsApiClient.Management_GetSiteItemIdPaged(nextStartId, 1000);
                lastAnyUrls = items.Count > 0;
                nextStartId = (items.LastOrDefault()?.Id ?? -2) + 1;

                items = items.Where(t => t.Path.EndsWith(".html")).ToList();
                counter += items.Count;

                siteItems.AddRange(items);

                if (items.Count > 0)
                {
                    logger.LogTrace("doindexnow starting submitting urls, site item range: [{0}, {1}]", items.First().Id, items.Last().Id);

                    string[] urls = items.Select(t => t.FullUri).ToArray();
                    var indexNowLog = new IndexNowLog(items.First().Id, items.Last().Id, true, null, DateTime.UtcNow.Date, 1);

                    try
                    {
                        await indexNowApi.SubmitUrls(urls);
                        logger.LogTrace("doindexnow submit urls success");
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "failed to send indexnow request");
                        indexNowLog.Success = false;
                        indexNowLog.LastException = Vinca.Utils.Helpers.ExceptionToStringForLogs(e);
                    }

                    await indexNowRepository.CreateAsync(indexNowLog);
                    await uow.SaveChangesAsync();

                    if (!indexNowLog.Success) return;
                }

            } while (lastAnyUrls && counter < 1000000);
        }

        private bool isStopping = false;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!isDisposed)
            {
                Dispose(true);
                logger.Log(LogLevel.Information, "On before stopping backend background service");

                BgWork_SaveLogs(null);

                cancellationTokenSource.Cancel();
            }

            // wait max 10 seconds and shutdown anyway (even if something is running)
            //for (int i = 0; i < 100 && IsAnythingRunning; i++) Thread.Sleep(100);

            //logger.Log(LogLevel.Information, "on after stopping backend background service");
            //if (IsAnythingRunning)
            //    logger.LogCritical("Something is running in background but force shutdown anyway. Not sure what to do with this for now. ");

            //return Task.CompletedTask;
        }

        //private void RemoveOldCache()
        //{
        //    logger.LogTrace("Starting RemoveOldCache");

        //    var cacheRepo = appUow.GetSimpleRepository<Cache>();

        //    var toDelete = appUow.GetSimpleRepository<Cache>()
        //        .Query()
        //        .Where(t => t.Expiration < DateTime.UtcNow)
        //        .Select(t => t.Id)
        //        .ToArray();

        //    logger.LogTrace($"RemoveOldCache, to delete ids: {toDelete.StringJoin(",")}");

        //    foreach (var id in toDelete)
        //    {
        //        cacheRepo.ExecuteDelete(t => t.Id == id);
        //    }
        //}

        private void BgWork_SaveLogs(object s)
        {
            try
            {
                var logs = ivBufferLogger.DequeueAllLogs();

                using var sqliteConnection = new SqliteConnection(RawRobiniaInfrastructure.LogDbConnectionString());
                sqliteConnection.Open();
                using var logCommand = sqliteConnection.CreateCommand();
                using var tx = sqliteConnection.BeginTransaction();

                logCommand.Transaction = tx;
                logCommand.CommandType = System.Data.CommandType.Text;

                foreach (var log in logs)
                {

                    var msg = log.Message == null ? "NULL" : $"'{log.Message.Replace("'", "''")}'";
                    logCommand.CommandText =
                    "INSERT INTO app_log(message, category_name, log_level_id, event_id, event_name, [date]) " +
                    $"VALUES ({msg}, '{log.CategoryName}', {(int)log.LogLevel}, {log.EventId.Id}, '{log.EventId.Name}', '{log.Date.ToString("O")}')";

                    logCommand.ExecuteNonQuery();
                }

                var httplogs = vHttpLogs.DequeueAll();

                using var httpLogsCommand = sqliteConnection.CreateCommand();
                httpLogsCommand.Transaction = tx;
                httpLogsCommand.CommandType = CommandType.Text;
                foreach (var hl in httplogs)
                {
                    httpLogsCommand.CommandText =
$@"
INSERT INTO http_log
(
start_date,
end_date,
log_write_date,
client_ip,
client_port,
method,
uri_path,
uri_query,
response_status,
bytes_send,
bytes_received,
time_taken_ms,
host,
user_agent,
referer
)
VALUES
(
'{hl.StartDate?.ToStringSql()}',
'{hl.EndDate?.ToStringSql()}',
'{DateTimeOffset.UtcNow.ToStringSql()}',
'{hl.ClientIP}',
{hl.ClientPort?.ToString() ?? "NULL"},
'{hl.Method}',
'{hl.UriPath?.Replace("'", "''")}',
'{hl.UriQuery?.Replace("'", "''")}',
{hl.ResponseStatus},
{hl.BytesSend?.ToString() ?? "NULL"},
{hl.BytesReceived?.ToString() ?? "NULL"},
{hl.TimeTakenMs},
'{hl.Host}',
'{hl.UserAgent}',
'{hl.Referer}'
);
";
                    httpLogsCommand.ExecuteNonQuery();

                }
                tx.Commit();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "system important exception");

                // question: what should  happen in this unhandled  exception in background worker service?
                // throw;
            }
        }

        public void RunBuildProjects() { BgWork_BuildProjects(null); }

        private void BgWork_BuildProjects(object _)
        {
            lock (_lock)
            {
                if (!taskBuildProjects.IsCompleted) return;
                taskBuildProjects = Task.Run(() => RunCommand(new BuildNugetOrgProjectCommand()));
            }
        }

        async Task RunCommand(ICommand command)
        {
            using (var scope = services.CreateScope())
            {
                try
                {
                    var cd = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

                    var result = await cd.DispatchAsync(command, cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "failed processing command");
                }
            }
        }

        ~ApiBackgroundWorker() { Dispose(false); }
        public void Dispose() { Dispose(true); }
        private bool isDisposed = false;

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                this.timerSaveLogs?.Dispose();
                this.timerBuildProjects?.Dispose();
            }
        }
    }
}

