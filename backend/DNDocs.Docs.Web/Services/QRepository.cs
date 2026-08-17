using Dapper;
using DNDocs.Docs.Web.Infrastructure;
using DNDocs.Docs.Web.Model;
using DNDocs.Docs.Web.ValueTypes;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace DNDocs.Docs.Web.Services
{
    public interface IQRepository
    {
        Task<SharedSiteItem> SelectSharedSiteItem(long id);

        // siteitem
        Task<SiteItem> SelectSiteItemAsync(long projectId, string path);
        Task<IEnumerable<SiteItem>> GetSiteItemPagedAsync(int pageNo, int rowsPerPage);
        Task<IEnumerable<SiteItem>> GetSiteItemIdPagedAsync(long startId, long count);
        Task<long> SelectSiteItemCount();

        Task<Project> SelectNugetProjectAsync(string nugetPackageName, string nugetPackageVersion);
        Task<Project> SelectSingletonProjectAsync(string urlPrefix);
        Task<Project[]> SelectProjectPagedAsync(int pageSize, int pageNo);

        // other
        Task<PublicHtml> SelectPublicHtml(string slug);

        //varsite
        Task<Sitemap> SelectSitemap(string path);
    }

    public class QRepository : IQRepository
    {
        private SqliteConnection CreateSiteDbConnection => infrastructure.OpenSqliteConnection(DatabaseType.Site);
        private SqliteConnection CreateVarSiteDbConnection => infrastructure.OpenSqliteConnection(DatabaseType.VarSite);
        private SqliteConnection CreateAppDbConnection => infrastructure.OpenSqliteConnection(DatabaseType.App);

        private IDInfrastructure infrastructure;
        private IDMetrics metrics;

        public QRepository(IDInfrastructure infrastructure, IDMetrics metrics)
        {
            this.infrastructure = infrastructure;
            this.metrics = metrics;
        }

        #region varsite
        public async Task<Sitemap> SelectSitemap(string path)
        {
            using var con = CreateVarSiteDbConnection;
            return await con.QueryFirstOrDefaultAsync<Sitemap>($"{SqlText.SelectSitemap} WHERE [path] = @path", new { path });
        }


        #endregion

        #region other

        public async Task<PublicHtml> SelectPublicHtml(string path)
        {
            using var con = infrastructure.OpenSqliteConnection(DatabaseType.VarSite);
            return await con.QueryFirstOrDefaultAsync<PublicHtml>("SELECT id as Id, [path] as [Path], byte_data as ByteData FROM public_html WHERE [path] = @path", new { @path });
        }

       
        #endregion

        #region SharedSiteItemDb

        public async Task<SharedSiteItem> SelectSharedSiteItem(long id)
        {
            var sw = Stopwatch.StartNew();
            using var con = infrastructure.OpenSqliteConnection(DatabaseType.VarSite);
            var sql = "SELECT id as Id, [path] as Path, byte_data as ByteData " +
                "FROM shared_site_item WHERE id  = @Id";
            
            var result = await con.QueryFirstOrDefaultAsync<SharedSiteItem>(sql, new { Id = id });
            metrics.SqlSelect($"{nameof(SelectSharedSiteItem)}", sw.ElapsedMilliseconds, result?.ByteData?.Length ?? 0);

            return result;
        }

        #endregion

        #region SiteItemDb

        public async Task<IEnumerable<SiteItem>> GetSiteItemIdPagedAsync(long startId, long limit)
        {
            using var con = infrastructure.OpenSqliteConnection(DatabaseType.Site);

            return await con.QueryAsync<SiteItem>($"{SqlSelectSiteItem(false)} WHERE id >= {startId} ORDER BY id ASC LIMIT {limit}");
        }

        public async Task<long> SelectSiteItemCount()
        {
            using var con = infrastructure.OpenSqliteConnection(DatabaseType.Site);
            return await con.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM site_item");
        }

        public async Task<SiteItem> SelectSiteItemAsync(long projectId, string path)
        {
            var sw = Stopwatch.StartNew();
            using var con = infrastructure.OpenSqliteConnection(DatabaseType.Site);
            var sql = $"{SqlSelectSiteItem(true)} WHERE project_id = @ProjectId AND [path] = @Path COLLATE NOCASE";
            var result = await con.QuerySingleOrDefaultAsync<SiteItem>(sql, new { ProjectId = projectId, Path = path });

            metrics.SqlSelect($"{nameof(SelectSiteItemAsync)}", sw.ElapsedMilliseconds, result?.ByteData?.Length ?? 0);
            return result;
        }

        //public async Task<SiteItem> SelectSiteItemUsingShared(long projectId, string path)
        //{
        //    using var connection = CreateSiteDbConnection;
        //    var sql = $"SELECT si.id as Id, si.project_id as ProjectId, si.[path] as Path, " +
        //        "(CASE WHEN si.shared_site_item_id IS NOT NULL THEN ssi.byte_data ELSE si.byte_data END) as ByteData " + 
        //        "FROM  site_item si " +
        //        "LEFT JOIN shared_site_item ssi on ssi.id = si.shared_site_item_id " +
        //        "WHERE si.project_id = @ProjectId AND si.[path] = @Path";

        //    return await connection.QuerySingleOrDefaultAsync<SiteItem>(sql, new { ProjectId = projectId, Path = path });
        //}

        public async Task<IEnumerable<SiteItem>> GetSiteItemPagedAsync(int pageNo, int pageSize)
        {
            using var connection = infrastructure.OpenSqliteConnection(DatabaseType.Site);
            var sql = $"{SqlSelectSiteItem(false)} ORDER BY id ASC LIMIT @Limit OFFSET @Offset";
            return await connection.QueryAsync<SiteItem>(sql, new { Offset = pageNo * pageSize, Limit = pageSize });
        }

        #endregion

        #region Project

        public async Task<Project> SelectSingletonProjectAsync(string urlPrefix)
        {
            using var connection = infrastructure.OpenSqliteConnection(DatabaseType.App);
            var sql = $"{SqlText.SelectProject} WHERE url_prefix = @UrlPrefix";
            return await connection.QuerySingleOrDefaultAsync<Project>(sql, new { UrlPrefix = urlPrefix });
        }

        public async Task<Project> SelectNugetProjectAsync(string nugetPackageName, string nugetPackageVersion)
        {
            using var connection = infrastructure.OpenSqliteConnection(DatabaseType.App);
            var sql = $"{SqlText.SelectProject} " +
                "WHERE project_type = @ProjectType " + 
                "AND nuget_package_name = @NugetPackageName COLLATE NOCASE " +
                "AND nuget_package_version = @NugetPackageVersion COLLATE NOCASE";
            return await connection.QuerySingleOrDefaultAsync<Project>(sql, new { NugetPackageName = nugetPackageName, NugetPackageVersion = nugetPackageVersion, ProjectType = ProjectType.Nuget });
        }

        public async Task<Project[]> SelectProjectPagedAsync(int pageNo, int pageSize)
        {
            using var connection = infrastructure.OpenSqliteConnection(DatabaseType.App);
            var sql = $"{SqlText.SelectProject} ORDER BY id DESC LIMIT @Limit OFFSET @Offset";
            var result = await connection.QueryAsync<Project>(sql, new { Limit = pageSize, Offset = pageNo * pageSize });

            return result.ToArray();
        }

        #endregion

        static string SqlSelectSiteItem(bool includeData)
        {
            var sql = "SELECT id as Id, project_id as ProjectId, [path] as Path, shared_site_item_id AS SharedSiteItemId ";
            if (includeData) sql += ", byte_data as ByteData ";
            else sql += ", NULL as ByteData ";

            sql += "FROM site_item";

            return sql;
        }

        // todo remove this and add conditions in method: bool includebytedata ... etc
        //        const string SqlSelectSiteItem =
        //@$"{SqlSelectSiteItem_NoData_NoFROM}, byte_data as ByteData FROM site_item";

        //        const string SqlSelectSiteItem_NoData_NoFROM =
        //@"SELECT id as Id, project_id as ProjectId, [path] as Path";

        //        const string SqlSelectSiteItem_NoData =
        //@$"{SqlSelectSiteItem_NoData_NoFROM} FROM site_item";

    }

    //static class SqliteExtensions
    //{
    //    public static async Task <IList<T>> QueryListAsync<T>(this SqliteConnection connection, string sql, object param)
    //    {
    //        return (await connection.QueryAsync<T>(sql, param)).ToList();
    //    }
    //}
}