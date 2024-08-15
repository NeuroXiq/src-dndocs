using Dapper;
using DNDocs.Docs.Web.Infrastructure;
using DNDocs.Docs.Web.Model;
using DNDocs.Docs.Web.Shared;
using DNDocs.Docs.Web.ValueTypes;
using Microsoft.Data.Sqlite;
using Microsoft.Net.Http.Headers;
using SQLitePCL;
using Microsoft.Data.Sqlite;
using System.Drawing;
using System.Runtime.CompilerServices;
using Vinca.Http.Logs;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;

namespace DNDocs.Docs.Web.Services
{
    public interface ITxRepository : IDisposable
    {
        void BeginTransaction();
        Task CommitAsync();
        Task RollbackAsync();

        Task InsertSharedSiteItem(SharedSiteItem newShared);

        Task InsertSiteHtmlAsync(SiteItem item);
        Task<Project> SelectProjectByIdAsync(long id);
        Task UpdateProjectAsync(Project project);
        Task InsertProjectAsync(Project project);

        Task InsertHttpLogAsync(IEnumerable<VHttpLog> logs);
        Task<Project> SelectVersionProject(string urlPrefix, string version);
        Task<Project> SelectSingletonProjectAsync(string urlPrefix);
        Task DeleteSiteHtmlByProjectIdAsync(long projectId);
        Task InsertAppLogAsync(IEnumerable<AppLog> logRows);
        
        Task<IEnumerable<string>> SelectSiteItemPathByProjectId(long projectId);
        Task DeleteProjectAsync(long projectId);
        Task<long?> SelectSharedSiteItemIdBySha256(string sha256);
        Task InsertResourceMonitorUtilization(ResourceMonitorUtilization rmu);

        // varsite
        Task InsertPublicHtml(PublicHtml publicHtml);
        Task InsertSitemapProject(IEnumerable<SitemapProject> sitemapProject);
        Task UpdatePublicHtml(PublicHtml publicHtml);
        Task<PublicHtml> SelectPublicHtmlByPath(string path);
        Task<IEnumerable<PublicHtml>> SelectAllSitemap();

        // other
        Task<IEnumerable<long>> ScriptForSitemapGenerator();

        // metrics
        Task InsertMtInstrument(MtInstrument newInstrument);
        Task InsertMtHRange(MtHRange mtHRange);
        Task InsertMtMeasurement(IEnumerable<MtMeasurement> mtMeasurements);
        Task<IEnumerable<MtInstrument>> SelectMtInstrument();
        Task<IEnumerable<MtHRange>> SelectMtHRange();
    }

    /// <summary>
    /// everyting same as DRepository but using transactions, must register as scoped
    /// </summary>
    public class TxRepository : ITxRepository
    {
        SqliteConnection GetLogSqliteConnection => GetSqliteConnection(DatabaseType.Log);

        SqliteConnection appConnection = null;
        SqliteConnection siteConnection = null;
        SqliteConnection logConnection = null;
        SqliteConnection varSiteConnection = null;
        SqliteTransaction appTx = null;
        SqliteTransaction siteTx = null;
        SqliteTransaction logTx = null;
        SqliteTransaction varSiteTx = null;

        bool isDisposed = false;
        bool isBeginTx = false;
        bool commitOrRollback = false;
        IDInfrastructure infrastructure;
        private IDMetrics metrics;

        public TxRepository(IDInfrastructure infrastructure, IDMetrics metrics)
        {
            this.infrastructure = infrastructure;
            this.metrics = metrics;
        }

        #region metrics

        public async Task<IEnumerable<MtInstrument>> SelectMtInstrument()
        {
            var con = GetSqliteConnection(DatabaseType.Log);

            return await con.QueryAsync<MtInstrument>(SqlText.SelectMtMeasurement);
        }

        public async Task<IEnumerable<MtHRange>> SelectMtHRange()
        {
            var con = GetSqliteConnection(DatabaseType.Log);
            string sql = "SELECT id as Id, mt_instrument_id as MtInstrumentId, end as End FROM mt_hrange";

            return await con.QueryAsync<MtHRange>(sql);
        }

        public async Task InsertMtMeasurement(IEnumerable<MtMeasurement> mtMeasurements)
        {
            var con = GetSqliteConnection(DatabaseType.Log);
            var sql = "INSERT INTO mt_measurement(mt_instrument_id, [value], mt_hrange_id, created_on) " +
                "VALUES (@MtInstrumentId, @Value, @MtHRangeId, @CreatedOn)";

            await con.ExecuteAsync(sql, mtMeasurements);
        }

        public async Task InsertMtHRange(MtHRange mtHRange)
        {
            var con = GetSqliteConnection(DatabaseType.Log);
            var sql = $"INSERT INTO mt_hrange(mt_instrument_id, [end]) VALUES (@MtInstrumentId, @End); " + 
                $"{SqlText.SelectLastInsertRowId};";

            mtHRange.Id = await con.ExecuteScalarAsync<int>(sql, mtHRange);
        }

        public async Task InsertMtInstrument(MtInstrument instrument)
        {
            var con = GetSqliteConnection(DatabaseType.Log);
            var sql = $"INSERT INTO mt_instrument([name], meter_name, instance_id, created_on, tags, type) " +
                $"VALUES(@Name, @MeterName, @InstanceId, @CreatedOn, @Tags, @Type); {SqlText.SelectLastInsertRowId}";

            instrument.Id = await con.ExecuteScalarAsync<int>(sql, instrument);
        }

        #endregion

        #region other

        public async Task<IEnumerable<long>> ScriptForSitemapGenerator()
        {
            // do not use GetSqliteConnection
            // not investigated why
            // but lock database/locks connection 
            // and is impossible to open & insert data in varsite dabatase
            // thus createproject requests hangs infinitely
            // need carefully investigate whats happen (maybe transaction + attach database gives problem?)
            // with separate connection seems to work
            // var conn = GetSqliteConnection(DatabaseType.VarSite);

            // doing without transaction and on separate connection
            var conn = this.infrastructure.OpenSqliteConnection(DatabaseType.VarSite);
            var sql = $"{infrastructure.AttachDatabase(DatabaseType.App)} AS [appdb]; ";

            await conn.ExecuteAsync(sql);

            // second query:
            // delete valid sitemaps to compact them later after generation occurs for not existing (want to have multiple projects in single sitemap)
            // for now 10000000 (10MB) but maybe exceed in future
            sql =
                "DELETE FROM sitemap_project AS sm WHERE NOT EXISTS (SELECT * FROM [appdb].project p WHERE p.id = sm.project_id);" +
                "DELETE FROM sitemap_project AS sm WHERE sm.public_html_id NOT IN (SELECT id FROM public_html);" +
                "DELETE FROM public_html AS ph WHERE [path] like '/sitemaps/%' AND ph.id NOT IN (SELECT sm.public_html_id FROM sitemap_project sm);";

            await conn.ExecuteAsync(sql);

            sql = $"SELECT id FROM [appdb].project p " +
                "WHERE NOT EXISTS (SELECT sp.id FROM sitemap_project sp WHERE sp.project_id = p.id);";

            IEnumerable<long> needSitemaps = await conn.QueryAsync<long>(sql);

            await conn.ExecuteAsync("DETACH DATABASE [appdb]");

            try
            {
                conn.Close();
                conn.Dispose();
            }
            catch { }

            return needSitemaps;
        }

        public async Task InsertResourceMonitorUtilization(ResourceMonitorUtilization rmu)
        {
            var conn = GetSqliteConnection(DatabaseType.Log);
            var sql = $"INSERT INTO resource_monitor_utilization (" +
                "cpu_used_percentage, memory_used_in_bytes, " +
                "memory_used_percentage, date_time )" +
                "VALUES (@CpuUsedPercentage, @MemoryUsedInBytes, @MemoryUsedPercentage, @DateTime)";
            await conn.ExecuteAsync(sql, rmu);
        }

        #endregion

        #region logs

        public async Task InsertAppLogAsync(IEnumerable<AppLog> logs)
        {
            var connection = GetSqliteConnection(DatabaseType.Log);
            await connection.ExecuteAsync(
                "INSERT INTO app_log([message], category_name, log_level_id, event_id, event_name, [date]) VALUES (" +
                $"@{nameof(AppLog.Message)}," +
                $"@{nameof(AppLog.CategoryName)}," +
                $"@{nameof(AppLog.LogLevelId)}," +
                $"@{nameof(AppLog.EventId)}," +
                $"@{nameof(AppLog.EventName)}," +
                $"@{nameof(AppLog.Date)}" +
                ")",
                logs);
        }

        #endregion

        #region SiteItem

        public async Task InsertSiteHtmlAsync(SiteItem item)
        {
            var sw = Stopwatch.StartNew();
            var connection = GetSqliteConnection(DatabaseType.Site);
            var sql = "INSERT INTO site_item(project_id, [path], shared_site_item_id, byte_data) " +
                "VALUES (@ProjectId, @Path, @SharedSiteItemId, @ByteData)";

            metrics.SqlInsert($"{nameof(InsertSharedSiteItem)}", sw.ElapsedMilliseconds, item?.ByteData?.Length ?? 0);
            await connection.ExecuteAsync(sql, item);
        }

        public async Task<IEnumerable<string>> SelectSiteItemPathByProjectId(long projectId)
        {
            var connection = GetSqliteConnection(DatabaseType.Site);
            var sql = $"SELECT [path] FROM site_item WHERE project_id = @ProjectId";

            return await connection.QueryAsync<string>(sql, new { ProjectId = projectId });
        }

        public async Task DeleteSiteHtmlByProjectIdAsync(long projectId)
        {
            var connection = GetSqliteConnection(DatabaseType.Site);
            var sql = "DELETE FROM site_item WHERE project_id = @ProjectId";
            await connection.ExecuteAsync(sql, new { ProjectId = projectId });
        }

        #endregion

        #region SharedSiteItemDb

        public async Task InsertSharedSiteItem(SharedSiteItem newShared)
        {
            var sw = Stopwatch.StartNew();
            var connection = GetSqliteConnection(DatabaseType.VarSite);
            var sql = "INSERT INTO shared_site_item(path, byte_data, sha_256) " +
                "VALUES (@Path, @ByteData, @Sha256); select last_insert_rowid();";
            newShared.Id = await connection.ExecuteScalarAsync<long>(sql, newShared);

            metrics.SqlInsert($"{nameof(InsertSharedSiteItem)}", sw.ElapsedMilliseconds, newShared?.ByteData?.Length ?? 0);
        }

        public async Task<long?> SelectSharedSiteItemIdBySha256(string sha256)
        {
            var connection = GetSqliteConnection(DatabaseType.VarSite);
            var sql = $"select id from shared_site_item WHERE sha_256 = @Sha256";
            return await connection.QuerySingleOrDefaultAsync<long?>(sql, new { Sha256 = sha256 });
        }

        #endregion

        #region Project
        public async Task DeleteProjectAsync(long id)
        {
            var connection = GetSqliteConnection(DatabaseType.App);
            var sql = $"DELETE FROM project WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<Project> SelectVersionProject(string urlPrefix, string version)
        {
            var connection = GetSqliteConnection(DatabaseType.App);
            var sql = $"{SqlText.SelectProject} WHERE url_prefix = @UrlPrefix AND project_version = @ProjectVersion";
            var result = await connection.QueryFirstOrDefaultAsync<Project>(sql, new { UrlPRefix = urlPrefix, ProjectVersion = version });

            return result;
        }

        public async Task<Project> SelectSingletonProjectAsync(string urlPrefix)
        {
            var connection = GetSqliteConnection(DatabaseType.App);
            var sql = $"{SqlText.SelectProject} WHERE url_prefix = @UrlPrefix AND project_type = @ProjectType";
            return await connection.QuerySingleOrDefaultAsync<Project>(sql, new { UrlPrefix = urlPrefix, ProjectType = ProjectType.Singleton });
        }

        public async Task<Project> SelectNugetProjectAsync(string nugetPackageName, string nugetPackageVersion)
        {
            var connection = GetSqliteConnection(DatabaseType.App);
            var sql = $"{SqlText.SelectProject} WHERE nuget_package_name = @NugetPackageName AND nuget_package_version = @NugetPackageVersion AND project_type = @ProjectType";

            return await connection.QuerySingleOrDefaultAsync<Project>(
                sql,
                new { NugetPackageName = nugetPackageName, NugetPackageVersion = nugetPackageVersion, ProjectType = ProjectType.Nuget });
        }

        public async Task<Project> SelectProjectByIdAsync(long id)
        {
            var connection = GetSqliteConnection(DatabaseType.App);
            var sql = $"{SqlText.SelectProject} WHERE id = {id}";

            return await connection.QuerySingleOrDefaultAsync<Project>(sql);
        }

        public async Task UpdateProjectAsync(Project project)
        {
            var connection = GetSqliteConnection(DatabaseType.App);
            var sql = $"UPDATE project SET " +
                "dn_project_id = @DnProjectId, metadata = @Metadata, " +
                "url_prefix = @UrlPrefix, project_version = @ProjectVersion, " +
                "nuget_package_name = @NugetPackageName, nuget_package_version = @NugetPackageVersion, " +
                "project_type = @ProjectType, created_on = @CreatedOn, updated_on = @UpdatedOn " +
                $"WHERE id = {project.Id}";

            var affectedRows = await connection.ExecuteAsync(sql, project);
        }

        public async Task InsertProjectAsync(Project project)
        {
            var connection = GetSqliteConnection(DatabaseType.App);
            var sql = $"INSERT INTO project" +
                "(dn_project_id, metadata, url_prefix, project_version, nuget_package_name, " +
                "nuget_package_version, project_type, created_on, updated_on) " +
                "VALUES (@DnProjectId, @Metadata, @UrlPrefix, @ProjectVersion, @NugetPackageName, " +
                "@NugetPackageVersion, @ProjectType, @CreatedOn, @UpdatedOn); " +
                "select last_insert_rowid();";

            project.Id = await connection.ExecuteScalarAsync<long>(sql, project);
        }

        #endregion

        // varsit
        public async Task UpdatePublicHtml(PublicHtml publicHtml)
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            var affectedRows = await con.ExecuteAsync($"{SqlText.UpdatePublicHtml} WHERE id = @Id", publicHtml);

            DValidation.ThrowISE(affectedRows != 1, "after updated affected rows not equal 1");
        }

        public async Task<PublicHtml> SelectPublicHtmlByPath(string path)
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            return await con.QueryFirstOrDefaultAsync<PublicHtml>($"{SqlText.SelectPublicHtml} WHERE [path] = @path", new { path });
        }

        public async Task<IEnumerable<PublicHtml>> SelectAllSitemap()
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            return await con.QueryAsync<PublicHtml>($"{SqlText.SelectPublicHtml_NoData} WHERE [path] LIKE '/sitemaps/%'");
        }

        public async Task InsertSitemapProject(IEnumerable<SitemapProject> sitemapProject)
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            await con.ExecuteAsync("INSERT INTO sitemap_project(project_id, public_html_id) VALUES (@ProjectId, @PublicHtmlId)", sitemapProject);
        }

        public async Task InsertPublicHtml(PublicHtml publicHtml)
        {
            await InsertPublicHtml(GetSqliteConnection(DatabaseType.VarSite), publicHtml);
        }

        public async Task InsertHttpLogAsync(IEnumerable<VHttpLog> logs)
        {
            var connection = GetSqliteConnection(DatabaseType.Log);
            var sql = $"INSERT INTO http_log " +
                $"([start_date], [end_date], [write_log_date], client_ip, client_port, method, uri_path, uri_query, response_status, bytes_send, " +
                "bytes_received, time_taken_ms, host, user_agent, referer) VALUES " +
                "(@StartDate, @EndDate, @WriteLogDate, @ClientIP, @ClientPort, @Method, @UriPath, @UriQuery, @ResponseStatus, @BytesSend, " +
                "@BytesReceived, @TimeTakenMs, @Host, @UserAgent, @Referer)";

            foreach (var log in logs)
            {
                log.WriteLogDate = DateTimeOffset.UtcNow;
            }

            await connection.ExecuteAsync(sql, logs);
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;


            if (isBeginTx && !commitOrRollback)
            {
                var transactions = new SqliteTransaction[] { appTx, siteTx, logTx };
                foreach (var tx in transactions)
                {
                    try { tx?.Rollback(); } catch { }
                }
            }

            appConnection?.Close();
            siteConnection?.Close();
            logConnection?.Close();
            varSiteConnection?.Close();

            //appConnection?.Dispose();
            //siteConnection?.Dispose();
            //logConnection?.Dispose();
            //varSiteConnection?.Dispose();

            //appTx?.Dispose();
            //siteTx?.Dispose();
            //logTx?.Dispose();
            //varSiteTx?.Dispose();
        }

        private SqliteConnection GetSqliteConnection(DatabaseType databaseType)
        {
            GetCurrentTxConnection(databaseType, out var connection, out var _);

            if (connection != null) return connection;

            connection = infrastructure.OpenSqliteConnection(databaseType);

            switch (databaseType)
            {
                case DatabaseType.App:
                    appConnection = connection;
                    appTx = connection.BeginTransaction();
                    break;
                case DatabaseType.Site:
                    siteConnection = connection;
                    siteTx = connection.BeginTransaction();
                    break;
                case DatabaseType.Log:
                    logConnection = connection;
                    logTx = connection.BeginTransaction();
                    break;
                case DatabaseType.VarSite:
                    varSiteConnection = connection;
                    varSiteTx = connection.BeginTransaction();
                    break;
                default: throw new ArgumentException();
            }

            return connection;
        }

        private void GetCurrentTxConnection(DatabaseType type, out SqliteConnection connection, out SqliteTransaction transaction)
        {
            switch (type)
            {
                case DatabaseType.App: connection = appConnection; transaction = appTx; break;
                case DatabaseType.Site: connection = siteConnection; transaction = siteTx; break;
                case DatabaseType.Log: connection = logConnection; transaction = logTx; break;
                case DatabaseType.VarSite: connection = varSiteConnection; transaction = varSiteTx; break;
                default: throw new ArgumentException();
            }
        }

        public void BeginTransaction()
        {
            DValidation.ThrowISE(isBeginTx, "already begin tx, need to commit/rollback before open new");
            
            isBeginTx = true;
            commitOrRollback = false;
        }

        public async Task RollbackAsync()
        {
            DValidation.ThrowISE(!isBeginTx, "not transaction begin");
            DValidation.ThrowISE(commitOrRollback, "already commited or rolledback");

            if (appTx != null) await appTx.RollbackAsync();
            if(siteTx != null) await siteTx.RollbackAsync();
            if(logTx != null) await logTx.RollbackAsync();
            if(varSiteTx != null) await varSiteTx.RollbackAsync();

            commitOrRollback = true;
        }

        public async Task CommitAsync()
        {
            DValidation.ThrowISE(!isBeginTx, "not transaction begin");
            DValidation.ThrowISE(commitOrRollback, "already commited or rolledback");

            if (appTx != null) await appTx.CommitAsync();
            if(siteTx != null) await siteTx.CommitAsync();
            if(logTx != null) await logTx.CommitAsync();
            if(varSiteTx != null) await varSiteTx.CommitAsync();

            commitOrRollback = true;
        }

        internal static async Task InsertPublicHtml(SqliteConnection connection, PublicHtml publicHtml)
        {
            publicHtml.Id = await connection.ExecuteScalarAsync<long>("INSERT INTO public_html([path], byte_data, created_on, updated_on) " +
                "VALUES(@Path, @ByteData, @CreatedOn, @UpdatedOn); select last_insert_rowid();", publicHtml);
        }

        internal static void InsertOrUpdatePublicHtmlFile(SqliteConnection connection, string path, byte[] byteData)
        {
            var existing = connection.QuerySingleOrDefault<PublicHtml>($"{SqlText.SelectPublicHtml} WHERE [path] = @path", new { path });

            if (existing == null)
            {
                InsertPublicHtml(connection, new PublicHtml(path, byteData)).Wait();
            }
            else
            {
                existing.ByteData = byteData;
                connection.Execute($"{SqlText.UpdatePublicHtml} WHERE id = @Id", existing);
            }
        }
    }
}
