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
        Task InsertSitemap(Sitemap sitemap);
        Task InsertPublicHtml(PublicHtml publicHtml);
        Task InsertSitemapProject(IEnumerable<SitemapProject> sitemapProject);
        Task UpdatePublicHtml(PublicHtml publicHtml);
        Task DeleteSitemapIndex();
        Task<IEnumerable<Sitemap>> SelectAllSitemap();

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
        bool isTransactionOpen = false;
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
            var sql = $"{infrastructure.GetAttachDatabaseSql(DatabaseType.App, "[appdb]")}";

            await conn.ExecuteAsync(sql);

            // delete small sitemaps to merge them after generation
            // because generator will generate everything what not exists 
            // and want to have as much big sitemaps as possible (means as close 50000 urls in single file)
            // but can occur that sitemap has only e.g. 1000urls in single file. thus delete this small files
            sql = "DELETE FROM sitemap WHERE " +
                "[path] <> '/sitemap.xml' AND " +
                "length(byte_data) < 2000000 " + 
                "AND urls_count < 25000 " +
                "AND (SELECT COUNT(*) FROM sitemap WHERE length(byte_data) < 2000000 AND urls_count < 25000) > 10; ";

            await conn.ExecuteAsync(sql);

            sql =
                $"DELETE FROM sitemap_project AS sm WHERE sm.project_id NOT IN (SELECT id FROM [appdb].project);" +
                $"DELETE FROM sitemap_project AS sm WHERE sm.sitemap_id NOT IN (SELECT id FROM sitemap);" +
                $"DELETE FROM sitemap WHERE id NOT IN (SELECT sitemap_id FROM sitemap_project) AND [path] <> '/sitemap.xml'";

            await conn.ExecuteAsync(sql);

            sql = $"SELECT p.id FROM [appdb].project p WHERE p.id NOT IN (SELECT sm.project_id FROM sitemap_project sm);";

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

        // varsite
        public async Task InsertSitemap(Sitemap sitemap)
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            var sql = $"INSERT INTO sitemap([path], decompressed_length, urls_count, byte_data, updated_on) " + 
                "VALUES (@Path, @DecompressedLength, @UrlsCount, @ByteData, @UpdatedOn); " +
                $"{SqlText.SelectLastInsertRowId};";

            sitemap.Id = await con.ExecuteScalarAsync<long>(sql, sitemap);
        }

        public async Task UpdatePublicHtml(PublicHtml publicHtml)
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            var affectedRows = await con.ExecuteAsync($"{SqlText.UpdatePublicHtml} WHERE id = @Id", publicHtml);

            DValidation.ThrowISE(affectedRows != 1, "after updated affected rows not equal 1");
        }

        public async Task DeleteSitemapIndex()
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            await con.ExecuteAsync($"DELETE FROM sitemap WHERE [path] = '/sitemap.xml'");
        }

        public async Task<IEnumerable<Sitemap>> SelectAllSitemap()
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            var sql = $"{SqlText.SelectSitemap_NoData} WHERE path <> '/sitemap.xml'";
            
            return await con.QueryAsync<Sitemap>(sql);
        }

        public async Task InsertSitemapProject(IEnumerable<SitemapProject> sitemapProject)
        {
            var con = GetSqliteConnection(DatabaseType.VarSite);
            await con.ExecuteAsync("INSERT INTO sitemap_project(project_id, sitemap_id) VALUES (@ProjectId, @SitemapId)", sitemapProject);
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


            if (isTransactionOpen)
            {
                var transactions = new SqliteTransaction[] { appTx, siteTx, logTx };
                foreach (var tx in transactions)
                {
                    try { tx?.Rollback(); } catch { }
                }
            }

            isTransactionOpen = false;

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
            DValidation.ThrowISE(isTransactionOpen, "already begin tx, need to commit/rollback before open new");

            isTransactionOpen = true;
        }

        public async Task RollbackAsync()
        {
            DValidation.ThrowISE(!isTransactionOpen, "not transaction begin");

            if (appTx != null) await appTx.RollbackAsync();
            if(siteTx != null) await siteTx.RollbackAsync();
            if(logTx != null) await logTx.RollbackAsync();
            if(varSiteTx != null) await varSiteTx.RollbackAsync();
            
            appTx = siteTx = logTx = varSiteTx = null;

            isTransactionOpen = false;
        }

        public async Task CommitAsync()
        {
            DValidation.ThrowISE(!isTransactionOpen, "not transaction begin");

            if (appTx != null) await appTx.CommitAsync();
            if(siteTx != null) await siteTx.CommitAsync();
            if(logTx != null) await logTx.CommitAsync();
            if(varSiteTx != null) await varSiteTx.CommitAsync();

            appTx = siteTx = logTx = varSiteTx = null;

            isTransactionOpen = false;
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
