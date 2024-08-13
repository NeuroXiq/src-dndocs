using Dapper;
using DNDocs.Docs.Web.Infrastructure;
using DNDocs.Docs.Web.Model;
using System.Collections.Concurrent;
using Vinca.Http.Logs;
using Vinca.BufferLogger;
using System.Diagnostics;

namespace DNDocs.Docs.Web.Services
{
    public interface ILogsService
    {
        Task BufferLoggerSaveLogs();
        Task SaveHttpLogsAsync();
    }

    public class LogsService : ILogsService
    {
        private IDMetrics metrics;
        private IVHttpLogService httpLogService;
        private IVBufferLogger bufferLogger;
        private IDInfrastructure infrastructure;
        private ILogger<LogsService> logger;
        private IServiceProvider serviceProvider;
        private IVBufferLogger vTickLoggerService;

        public LogsService(
            IDInfrastructure infrastructure,
            IServiceProvider serviceProvider,
            IVBufferLogger vTickLoggerService,
            ILogger<LogsService> logger,
            IVBufferLogger bufferLogger,
            IVHttpLogService httpLogService,
            IDMetrics metrics)
        {
            this.metrics = metrics;
            this.httpLogService = httpLogService;
            this.bufferLogger = bufferLogger;
            this.infrastructure = infrastructure;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.vTickLoggerService = vTickLoggerService;
        }

        public async Task SaveHttpLogsAsync()
        {
            var logs = httpLogService.DequeueAll();
            int logsCount = logs.Count();

            if (logsCount == 0) return;
            metrics.SaveHttpLogsCount(logsCount);
            
            logger.LogTrace("save http logs, count: {0}", logsCount);

            using var scope = serviceProvider.CreateScope();
            using var repository = scope.ServiceProvider.GetRequiredService<ITxRepository>();
            repository.BeginTransaction();

            await repository.InsertHttpLogAsync(logs);
            await repository.CommitAsync();
        }

        public async Task BufferLoggerSaveLogs()
        {
            if (bufferLogger.IsEmpty) return;
            var logRows = bufferLogger.DequeueAllLogs();
            metrics.SaveAppLogsCount(logRows.Count);

            using var scope = serviceProvider.CreateScope();
            using var repository = scope.ServiceProvider.GetRequiredService<ITxRepository>();
            var appLogs = logRows.Select(AppLog.FromVLog).ToList();

            repository.BeginTransaction();
            await repository.InsertAppLogAsync(appLogs);
            await repository.CommitAsync();
        }
    }
}
