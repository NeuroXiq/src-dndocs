using DNDocs.Docs.Web.Model;
using DNDocs.Docs.Web.Services;
using DNDocs.Docs.Web.Shared;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Vinca.BufferLogger;
using Vinca.Http.Logs;
using Vinca.SitemapXml;
using Vinca.Utils;

namespace DNDocs.Docs.Web.Application
{
    public class DHostedService : IHostedService
    {
        private Timer logsTimer;
        private Timer resourceMonitorTimer;
        private Timer generateSitemapsTimer;
        private Timer metricTimer;
        private Timer metricsTimer;
        private IDMetrics metrics;
        private DSettings settings;
        private IServiceProvider serviceProvider;
        private ILogsService logsService;
        private ILogger<DHostedService> logger;
        private IResourceMonitor resourceMonitor;
        private int isSystemThreadRun;
        CancellationTokenSource cancelAllTokenSource;

        Task saveLogsTask = Task.CompletedTask;
        Task saveHttpLogsTask = Task.CompletedTask;
        Task saveMetricsTask = Task.CompletedTask;
        Task generateSitemapsTask = Task.CompletedTask;

        public DHostedService(
            IServiceProvider serviceProvider,
            ILogsService logsService,
            IResourceMonitor resourceMonitor,
            ILogger<DHostedService> logger,
            IDMetrics metrics,
            IOptions<DSettings> settings)
        {
            this.metrics = metrics;
            this.settings = settings.Value;
            this.serviceProvider = serviceProvider;
            this.logsService = logsService;
            this.logger = logger;
            this.resourceMonitor = resourceMonitor;

            isSystemThreadRun = 0;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("starting");
            cancelAllTokenSource = new CancellationTokenSource();

            logsTimer = new Timer(LogsTimerCallback, null, settings.FlushAllLogsTimeSpan, settings.FlushAllLogsTimeSpan);
            generateSitemapsTimer = new Timer(GenerateSitemapsCallback, null, TimeSpan.FromSeconds(3), settings.TimespanGenerateSitemapPeriod);
            metricTimer = new Timer(OnMetricsTimer, null, TimeSpan.FromSeconds(1), settings.TimeSpanSaveMetrics);

            // systemWorkTimer = new Timer(DoSystemWorkTimerCallback, null, 5, 2000 );
            // throw new NotImplementedException();
        }

        private void OnMetricsTimer(object state)
        {
            if (saveMetricsTask.IsCompleted)
            {
                saveMetricsTask = Task.Run(SaveMetrics);
            }
        }

        async Task SaveMetrics()
        {
            try
            {
                await metrics.SaveInDbAndClear();
            }
            catch (Exception e)
            {
                logger.LogError(e, "failed to save metrics");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("stopping");
            logsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            generateSitemapsTimer.Change(Timeout.Infinite, Timeout.Infinite);
            metricsTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // todo everything must have cancellationtoken to stop in conrolled/success way
            var runningJobs = Task.WhenAll(saveLogsTask, saveHttpLogsTask, saveMetricsTask);

            // wait max 15 seconds and force abort (?)
            // maybe other solution? how long to wait, how abort all other safely?
            await Task.WhenAny(Task.Delay(15000), runningJobs);
        }

        void LogsTimerCallback(object _)
        {
            if (saveLogsTask.IsCompleted) saveLogsTask = Task.Run(logsService.BufferLoggerSaveLogs);
            if (saveHttpLogsTask.IsCompleted) saveHttpLogsTask = Task.Run(logsService.SaveHttpLogsAsync);
        }

        void DoWork()
        {
            // work to do:
            // 1. ccheck sector sizes
            // 2. to delete projects remove
            // 3. backup azure
            // 4. generate sitemaps
            // 5. check problems: project is in app.sqlite not in site.sqlite
            // 6.  pragma wal_checkpoint
        }

        private async Task DoSystemWorkThreadStart()
        {
            await GenerateSitemaps();
        }

        private async Task TransactionWrap(Func<IServiceProvider, ITxRepository, Task> action)
        {
            using var scope = serviceProvider.CreateScope();
            using var repository = scope.ServiceProvider.GetRequiredService<ITxRepository>();

            try
            {
                repository.BeginTransaction();
                await action(scope.ServiceProvider, repository);
                await repository.CommitAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                repository.Dispose();
            }
        }

        private async Task DbCleanup()
        {
            //vacuum: 100% disk usage  on two 2GB databases
            // 1. is this needed to vacuum? on the internet said that 
            // 2. is checkpoint needed?
            // actually maybe if will be fine without this, as not too much
            // deletes are expected to be
            //
            // this is just optimalization not 
            //using var appConnection = infrastructure.CreateSqliteConnection(DatabaseType.App);
            //using var siteConnection = infrastructure.CreateSqliteConnection(DatabaseType.Site);
            //using var logConnection = infrastructure.CreateSqliteConnection(DatabaseType.Log);

            //await appConnection.ExecuteAsync("VACUUM");
            //await siteConnection.ExecuteAsync("VACUUM");
            //await logConnection.ExecuteAsync("VACUUM");

            //using var connLogs = CreateLogDbConnection;
            //var sql = $"delete from resource_monitor_utilization WHERE date_time < @Deprecated";
            //await connLogs.ExecuteAsync(sql, new { Deprecated = DateTime.UtcNow.AddDays(-7) });
            //await connLogs.ExecuteAsync("PRAGMA wal_checkpoint;");
            //await connLogs.ExecuteAsync("VACUUM");

            //using var connApp = CreateAppDbConnection;
            //await connApp.ExecuteAsync("PRAGMA wal_checkpoint;");
            //await connApp.ExecuteAsync("VACUUM");

            //using var connSite = CreateSiteDbConnection;
            //await connSite.ExecuteAsync("PRAGMA wal_checkpoint;");
            //await connSite.ExecuteAsync("VACUUM");
        }

        private void GenerateSitemapsCallback(object _)
        {
            if (!generateSitemapsTask.IsCompleted) return;

            generateSitemapsTask = Task.Run(GenerateSitemaps);
        }

        private async Task GenerateSitemaps()
        {
            logger.LogInformation("generate sitemaps");

            var sw = Stopwatch.StartNew();
            SitemapGenerator sitemapGenerator = new SitemapGenerator();

            // ScriptForSitemapGenerator not working ocrrectly
            // probably problem with attach  database
            // if trying to create  project on docs.dndocs then
            // indefinitely waits for open connection and hangs indefinitely
            // need to investigate why this locks databases:
            var scope = serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITxRepository>();
            repository.BeginTransaction();

            var needSitemapId = await repository.ScriptForSitemapGenerator();
            await repository.CommitAsync();

            List<long> projectsInSingleSitemap = new List<long>();

            if (needSitemapId.Count() == 0)
            {
                logger.LogInformation("no sitemap needed");
                return;
            }

            byte[] brotliBuffer = new byte[10000000];

            repository.BeginTransaction();

            foreach (var projectId in needSitemapId)
            {
                var p = await repository.SelectProjectByIdAsync(projectId);
                IList<string> urls = (await repository.SelectSiteItemPathByProjectId(projectId)).Where(url => url.EndsWith(".html")).ToList();

                switch (p.ProjectType)
                {
                    case ProjectType.Singleton: urls = urls.Select(u => settings.GetUrlSingletonProject(p.UrlPrefix, u)).ToList(); break;
                    case ProjectType.Version: urls = urls.Select(u => settings.GetUrlVersionProject(p.UrlPrefix, p.ProjectVersion, u)).ToList(); break;
                    case ProjectType.Nuget: urls = urls.Select(u => settings.GetUrlNugetOrgProject(p.NugetPackageName, p.NugetPackageVersion, u)).ToList(); break;
                    default: throw new NotImplementedException();
                }

                var now = DateTime.UtcNow;
                bool appended = sitemapGenerator.TryAppend(urls, DateTime.UtcNow, ChangeFreq.Monthly);
                
                if (appended)
                {
                    projectsInSingleSitemap.Add(p.Id);
                }
                else if (sitemapGenerator.UrlsCount == 0)
                {
                    logger.LogError("failed to generate sitemap for projectid: {0}, skipping this project without sitemap", projectId);
                    continue;
                }

                // restrict for not bigger than 10MB to have something to download in reasonable time (arbitrary size restriction)
                if (!appended || sitemapGenerator.CurrentLength > 10 * 1000 * 1000 || projectId == needSitemapId.Last())
                {
                    long urlsCount = sitemapGenerator.UrlsCount;
                    var sitemapXmlString = sitemapGenerator.ToXmlStringAndClear();
                    byte[] byteData = Encoding.UTF8.GetBytes(sitemapXmlString);
                    long decompressedLength = byteData.Length;
                    byteData = Shared.Helpers.BrotliCompress(byteData, ref brotliBuffer);

                    var sitemap = new Sitemap($"/public/sitemaps/sitemap_project_{Guid.NewGuid()}.xml", byteData.Length, urlsCount, byteData);
                    await repository.InsertSitemap(sitemap);
                    var sitemapProjects = projectsInSingleSitemap.Select(projectId => new SitemapProject(sitemap.Id, projectId));
                    
                    logger.LogTrace("inserting sitemap '{0}' for project_id: ({1})", sitemap.Path, projectsInSingleSitemap.StringJoin(","));
                    await repository.InsertSitemapProject(sitemapProjects);
                    await repository.CommitAsync();
                    repository.BeginTransaction();
                    projectsInSingleSitemap.Clear();
                }
            }

            IEnumerable<Sitemap> allSitemaps = await repository.SelectAllSitemap();
            var sitemapIndexGen = new SitemapIndexGenerator();

            foreach (var sitemapItem in allSitemaps)
                sitemapIndexGen.Append(settings.GetUrlDDocs(sitemapItem.Path), sitemapItem.UpdatedOn);

            await repository.DeleteSitemapIndex();
            long sitemapsCount = sitemapIndexGen.UrlsCount;
            byte[] sitemapIndexByteData = Encoding.UTF8.GetBytes(sitemapIndexGen.ToStringXmlAndClear());
            long decompressedLen = sitemapIndexByteData.Length;
            sitemapIndexByteData = Shared.Helpers.BrotliCompress(sitemapIndexByteData, ref brotliBuffer);
            
            var sitemapIndex = new Sitemap("/sitemap.xml", decompressedLen, sitemapsCount, sitemapIndexByteData);
            await repository.InsertSitemap(sitemapIndex);

            await repository.CommitAsync();
            sw.Stop();

            logger.LogInformation("completed generating sitemaps, project_id:({0}) total time: {1}s",
                needSitemapId.StringJoin(",", t => t.ToString()), sw.Elapsed.Seconds);
        }
    }
}