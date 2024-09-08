using Dapper;
using DbUp;
using DNDocs.Docs.Web.Services;
using DNDocs.Docs.Web.Shared;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Text;

namespace DNDocs.Docs.Web.Infrastructure
{
    public interface IDInfrastructure
    {
        // public 
        void ApplicationStartup();
        SqliteConnection OpenSqliteConnection(DatabaseType db);
        string GetAttachDatabaseSql(DatabaseType type, string alias);
    }

    public enum DatabaseType
    {
        App,
        Site,
        Log,
        VarSite
    }

    public class DInfrastructure : IDInfrastructure
    {
        private IDMetrics metrics;
        private DFileSystemOptions options;

        string ConnectionString_App, OSPath_AppDb;
        string ConnectionString_Site, OSPath_SiteDb;
        string ConnectionString_VarSite, OSPath_VarSiteDb;
        string ConnectionString_Log, OSPath_LogDb;

        public DInfrastructure(IOptions<DFileSystemOptions> fsOptions, IDMetrics metrics)
        {
            this.metrics = metrics;
            this.options = fsOptions.Value;
            if (!Directory.Exists(options.InfrastructureFolderOSPath))
                throw new ArgumentException($"(Safety): Directory for instrastructure not exists: '{options.InfrastructureFolderOSPath}'. Create this directory manually");

            OSPath_AppDb = Path.Combine(options.InfrastructureFolderOSPath, options.AppDbSqliteFileName);
            OSPath_SiteDb = Path.Combine(options.InfrastructureFolderOSPath, options.SiteDbSqliteFileName);
            OSPath_VarSiteDb = Path.Combine(options.InfrastructureFolderOSPath, options.VarSiteDbSqliteFileName);
            OSPath_LogDb = Path.Combine(options.InfrastructureFolderOSPath, options.LogDbSqliteFileName);

            ConnectionString_App = $"Data Source={OSPath_AppDb};";
            ConnectionString_Site = $"Data Source={OSPath_SiteDb};";
            ConnectionString_Log = $"Data Source={OSPath_LogDb};";
            ConnectionString_VarSite = $"Data Source={OSPath_VarSiteDb};";
        }

        public string GetAttachDatabaseSql(DatabaseType type, string alias)
        {
            string path = null;
            switch (type)
            {
                case DatabaseType.App: path = OSPath_AppDb; break;
                case DatabaseType.Site: path = OSPath_SiteDb; break;
                case DatabaseType.Log: path = OSPath_LogDb; break;
                case DatabaseType.VarSite: path = OSPath_VarSiteDb; break;
                default: throw new ArgumentException("type");
            }

            return $"ATTACH DATABASE '{path}' AS {alias}";
        }

        string GetConnectionString(DatabaseType dbType)
        {
            string connectionString;
            switch (dbType)
            {
                case DatabaseType.App: connectionString = ConnectionString_App; break;
                case DatabaseType.Log: connectionString = ConnectionString_Log; break;
                case DatabaseType.Site: connectionString = ConnectionString_Site; break;
                case DatabaseType.VarSite: connectionString = ConnectionString_VarSite; break;
                default: throw new ArgumentException(nameof(dbType));
            }

            return connectionString;
        }

        public SqliteConnection OpenSqliteConnection(DatabaseType db)
        {
            metrics.SqlOpen(db.ToString());
            string connectionString = GetConnectionString(db);

            var sqliteConnection = new SqliteConnection(connectionString);
            // SqliteConnection.BusyTimeout = 5 * 60 ( * 1000 ??)??;
            sqliteConnection.DefaultTimeout = 5 * 60;
#if DEBUG
            // on dev for investigate perf issues
            sqliteConnection.DefaultTimeout = 10;
#endif
            sqliteConnection.Execute("PRAGMA journal_size_limit=1000000000;"); //?
            sqliteConnection.Open();

            return sqliteConnection;
        }

        public void ApplicationStartup()
        {
            CreateDbIfNotExists(ConnectionString_App);
            CreateDbIfNotExists(ConnectionString_Site);
            CreateDbIfNotExists(ConnectionString_VarSite);
            CreateDbIfNotExists(ConnectionString_Log);

            RunMigration(ConnectionString_App, "DNDocs.Docs.Web.Infrastructure.Migrations.App.");
            RunMigration(ConnectionString_Site, "DNDocs.Docs.Web.Infrastructure.Migrations.Site.");
            RunMigration(ConnectionString_VarSite, "DNDocs.Docs.Web.Infrastructure.Migrations.VarSite.");
            RunMigration(ConnectionString_Log, "DNDocs.Docs.Web.Infrastructure.Migrations.Log.");

            string publicHtmlFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PublicHtml");
            string[] publicHtmlFiles = Directory.GetFiles(publicHtmlFolder, "*.*", SearchOption.AllDirectories);
            var con = OpenSqliteConnection(DatabaseType.VarSite);

            // todo: remove from db if not exists in PublicHtml folder
            foreach (var file in publicHtmlFiles)
            {
                string httpPath = file.Substring(publicHtmlFolder.Length).Replace("\\", "/");
                byte[] data = File.ReadAllBytes(file);
                TxRepository.InsertOrUpdatePublicHtmlFile(con, httpPath, data);
            }
        }

        static void CreateDbIfNotExists(string connStr)
        {
            using (var connection = new SqliteConnection(connStr))
            {
                connection.Open();
                connection.Close();
            }
        }

        static void RunMigration(string connectionString, string filter)
        {
            //RawRobiniaInfrastructure.CreateSqliteDbIfNotExists(connectionString);
            var asm = typeof(DInfrastructure).Assembly;

            var upgrader = DeployChanges.To
                .SQLiteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(asm, (resourceName) => resourceName.StartsWith(filter))
                .LogScriptOutput()
                // .WithPreprocessor(new TransactionProcessor())
                .LogToConsole()
                // .WithTransaction() <-- needed by 0008 migration
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw result.Error;
            }
        }
    }

    public static class InfrastructureExtensions
    {
        public static void AddDNDDInfrastructure(this IServiceCollection sc)
        {
            sc.AddSingleton<IDInfrastructure, DInfrastructure>();
        }

        public static void DInfrastructureAppBuilded(this WebApplication w)
        {
            var i = w.Services.GetRequiredService<IDInfrastructure>();
            i.ApplicationStartup();
        }
    }
}
