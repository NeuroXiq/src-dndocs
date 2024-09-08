using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using Microsoft.Extensions.Options;
using DNDocs.Application.Application;
using DNDocs.Application.Commands.Admin;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Shared.Configuration;
using DNDocs.Shared.Utils;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using DNDocs.Infrastructure.Utils;
using DNDocs.Infrastructure.DataContext;

namespace DNDocs.Application.CommandHandlers
{
    class DoBackgroundWorkHandler : CommandHandler<DoBackgroundWorkCommand>
    {
        private IServiceProvider serviceProvider;
        private ICommandDispatcher cd;
        private DNDocsSettings roptions;
        private IAppUnitOfWork appUow;
        private IProjectManager projectManager;
        private IAppManager appManager;
        private bool IsCancellationRequested => base.cancellationToken.IsCancellationRequested;
        public static ConcurrentBag<HttpLog> HttpLogs = new ConcurrentBag<HttpLog>();
        public static ConcurrentBag<AppLog> Logs = new ConcurrentBag<AppLog>();

        public static string StatusString = "";
        static object _lock = new object();

        static DoBackgroundWorkHandler()
        {
        }

        List<Action> todo;

        class ItemToDo
        {
            public Action Action;
            public TimeSpan WaitTime;
            public bool Force;

            public ItemToDo(Action action, TimeSpan wait, bool force)
            {
                Force = force;
                Action = action;
                WaitTime = wait;
            }
        }

        public DoBackgroundWorkHandler(
            IAppUnitOfWork appUow,
            IOptions<DNDocsSettings> options,
            ICommandDispatcher cd,
            IProjectManager projectManager,
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.cd = cd;
            this.roptions = options.Value;
            this.appUow = appUow;
            this.projectManager = projectManager;
            this.appManager = appManager;
        }

        public override void Handle(DoBackgroundWorkCommand command)
        {
            List<ItemToDo> todo = new List<ItemToDo>()
            {
                //new ItemToDo(
                //    DoGenerateSitemap,
                //    TimeSpan.FromDays(7),
                //    command.ForceGenerateSitemap),

                new ItemToDo(
                    RemoveOldCache,
                    TimeSpan.FromMinutes(15),
                    false),
            };

            if (todo.Any(t => t.Force)) todo = todo.Where(t => t.Force).ToList();

            foreach (var item in todo)
            {
                if (IsCancellationRequested) return;

                string lastRunSettingKey = $"DoBgWork_{item.Action.Method.Name}_LastRunDateTime";
                var sysVarRepo = appUow.GetSimpleRepository<SysVar>();
                var setting = sysVarRepo.Query().Where(t => t.Key == lastRunSettingKey).SingleOrDefault();
                DateTime lastRunTime = DateTime.UtcNow.AddYears(-1);

                if (setting == null)
                {
                    setting = new SysVar(lastRunSettingKey, DateTime.UtcNow.AddDays(-10).ToString("O"));
                    sysVarRepo.Create(setting);
                }

                lastRunTime = DateTime.Parse(setting.Value);

                bool run = item.Force || command.ForceAll || (lastRunTime + item.WaitTime) < DateTime.Now;
                if (!run) continue;

                try
                {
                    StatusString = item.Action.Method.Name;
                    item.Action();
                    setting.Value = DateTime.UtcNow.ToString("O");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "failed to process item");
                }
            }

            StatusString = "Not running";
        }

        private void RemoveOldCache()
        {
            logger.LogTrace("Starting RemoveOldCache");

            var cacheRepo = appUow.GetSimpleRepository<Cache>();

            var toDelete = appUow.GetSimpleRepository<Cache>()
                .Query()
                .Where(t => t.Expiration < DateTime.UtcNow)
                .Select(t => t.Id)
                .ToArray();

            logger.LogTrace($"RemoveOldCache, to delete ids: {toDelete.StringJoin(",")}");

            foreach (var id in toDelete)
            {
                cacheRepo.ExecuteDelete(t => t.Id == id);
            }
        }
       
        public void DoImportantWork()
        {
            AppLog[] appLogs;
            HttpLog[] httpLogs;

            lock (_lock)
            {
                appLogs = Logs.ToArray();
                httpLogs = HttpLogs.ToArray();

                HttpLogs.Clear();
                Logs.Clear();
            }

            var uow = appUow;
            var logsChunks = appLogs.Chunk(1000);
            var httpLogsChunks = httpLogs.Chunk(1000);

            foreach (var logChunk in logsChunks)
            {
                uow.AppLogRepository.Create(logChunk);

                Thread.Sleep(500);
            }

            foreach (var httpChunk in httpLogsChunks)
            {
                Thread.Sleep(500);
                uow.HttpLogRepository.Create(httpChunk);
            }

            uow.SaveChanges();
        }
    }
}
