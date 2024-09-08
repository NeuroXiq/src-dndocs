using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using DNDocs.Application.CommandHandlers;
using DNDocs.Application.Commands.Admin;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Shared.Configuration;
using System.Diagnostics;
using DNDocs.Application.Services;
using DNDocs.Shared.Utils;
using System.Reflection;
using DNDocs.Application.Commands.Projects;
using Vinca.BufferLogger;
using DNDocs.Infrastructure.Utils;
using Microsoft.Data.Sqlite;
using Vinca.Http.Logs;
using System.Data;
using Vinca.Utils;
using System;
using DNDocs.Docs.Api.Client;
using NuGet.Packaging;
using DNDocs.Docs.Api.Management;
using Vinca.Api;

namespace DNDocs.Application.Application
{
    public class ApiBackgroundWorker : IHostedService, IDisposable
    {
        static bool IsNormalRunning = false;
        static bool IsImportantRunning = false;

        static object _lock = new object();
        CancellationTokenSource cancellationTokenSource;
        private IBgJobQueue bgjobQueue;
        private readonly int SleepSecondsDoImportantWork;
        private readonly int SleepSecondsDoWork;
        private Timer timerImportantWork;
        private Timer timerWork;
        private Timer timerIndexNow;
        private IIndexNowApi indexNowApi;
        private IDDocsApiClient ddocsApiClient;
        private IVHttpLogService vHttpLogs;
        private IDNInfrastructure dinfrastructure;
        private IVBufferLogger ivBufferLogger;
        private IServiceProvider services;
        private ILogger<ApiBackgroundWorker> logger;
        private Task taskIndexNow = Task.CompletedTask;

        public ApiBackgroundWorker(IServiceProvider services,
            ILogger<ApiBackgroundWorker> logger,
            IOptions<DNDocsSettings> robiniaSettings,
            IBgJobQueue bgjobQueue,
            IVBufferLogger ivBufferLogger,
            IDNInfrastructure dinfrastructure,
            IVHttpLogService vHttpLogs,
            IDDocsApiClient ddocsApiClient,
            IIndexNowApi indexNowApi
            )
        {
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

            timerImportantWork = new Timer(TimerTickImportantWork, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(SleepSecondsDoImportantWork));
            timerWork = new Timer(TimerTickWork, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(SleepSecondsDoWork));
            timerIndexNow = new Timer(OnTimerIndexNow, null, TimeSpan.FromSeconds(5), TimeSpan.FromHours(24.1));

            await bgjobQueue.OnSystemStart();

            // 1. generate projects
            // 2. cleanup bgjob remote services
        }

        private void OnTimerIndexNow(object state)
        {
            if (!taskIndexNow.IsCompleted) return;
            taskIndexNow = Task.Run(DoIndexNow);
        }

        private async Task DoIndexNow()
        {
            logger.LogTrace("starting doindexnow");

            using var scope = services.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
            var indexNowRepository = uow.GetSimpleRepository<IndexNowLog>();

            List<SiteItemDto> siteItems = new List<SiteItemDto>();
            bool lastAnyUrls = true;
            int counter = 0;
            long nextStartId = indexNowRepository.Query().Any() ? indexNowRepository.Query().Max(t => t.SiteItemIdEnd) + 1 : 1;

            do
            {
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

            } while (lastAnyUrls && counter < 10000);
        }

        private bool isStopping = false;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!isDisposed)
            {
                Dispose(true);
                logger.Log(LogLevel.Information, "On before stopping backend background service");
                DoWork(WorkType.SystemImportant);

                cancellationTokenSource.Cancel();
            }

            // wait max 10 seconds and shutdown anyway (even if something is running)
            //for (int i = 0; i < 100 && IsAnythingRunning; i++) Thread.Sleep(100);

            //logger.Log(LogLevel.Information, "on after stopping backend background service");
            //if (IsAnythingRunning)
            //    logger.LogCritical("Something is running in background but force shutdown anyway. Not sure what to do with this for now. ");

            //return Task.CompletedTask;
        }

        DoBackgroundWorkCommand doBackgroundWorkCommand = null;

        public void DoSystemWorkNow(DoBackgroundWorkCommand command = null)
        {
            doBackgroundWorkCommand = command;
            TimerTickWork(null);
        }

        private void TimerTickImportantWork(object state)
        {
            DoWork(WorkType.SystemImportant);
        }

        private void TimerTickWork(object state)
        {
            DoWork(WorkType.SystemNormal);
        }

        enum WorkType
        {
            SystemNormal,
            SystemImportant
        }

        void DoWork(WorkType type)
        {
            lock (_lock)
            {
                if (type == WorkType.SystemImportant && IsImportantRunning) return;
                if (type == WorkType.SystemNormal && IsNormalRunning) return;

                if (type == WorkType.SystemImportant) IsImportantRunning = true;
                if (type == WorkType.SystemNormal) IsNormalRunning = true;
            }

            if (type == WorkType.SystemImportant)
            {

                // now this runs on Timer thread,
                // this is not correct way (timer operations should be very short, start thread or something and immediately return)
                // but for now it works so instead of creating new
                // thread everytime (saving logs should be short operation)
                // run directly on timer thread in 'incorrect' way
                // this should be invoked often because logs should be
                // save in db as fast as possible

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
'{hl.UriPath?.Replace("'",  "''")}',
'{hl.UriQuery?.Replace("'", "''")}',
{hl.ResponseStatus},
{hl.BytesSend?.ToString() ?? "NULL" },
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
                    IsImportantRunning = false;
                }
                catch (Exception e)
                {
                    IsImportantRunning = false;
                    logger.LogCritical(e, "system important exception");

                    // question: what should  happen in this unhandled  exception in background worker service?
                    // throw;
                }

                return;
            }

            try
            {
                var t = new Thread(ThreadHandlerWork);

                t.Priority = ThreadPriority.Normal;
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception)
            {
                if (type == WorkType.SystemNormal) IsNormalRunning = false;

                // question: what should  happen in this unhandled  exception in background worker service?
                //throw;
            }
        }

        void ThreadHandlerWork()
        {
            try
            {
                using (var scope = services.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();

                    try
                    {
                        var cd = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
                        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();

                        currentUser.AuthenticateAsUser(fromUserLogin: User.AdministratorUserLogin);

                        var result = cd.Dispatch(new BuildProjectCommand(), cancellationTokenSource.Token);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "failed processing buildproject command");
                    }
                    uow.SaveChanges();
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "failed to process bg job queue");
            }
            finally
            {
                IsNormalRunning = false;
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
                this.timerImportantWork?.Dispose();
                this.timerWork?.Dispose();
            }
        }
    }
}

