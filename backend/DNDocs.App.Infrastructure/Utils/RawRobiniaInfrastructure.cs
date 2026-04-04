// + <ROOT_Directory>
// | '''.. other files / folders in ROOT_Directory ... '''
// |
// + <robinia-infrastructure-files>
//    - appdb.sqlite
//    - projects 
//      - tenant-project-1-1.sqlite
//      - tenant-project-1-2.sqlite
//      - tenant-project-2-3.sqlite .... etc...
//
//

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Infrastructure.DataContext;
using DNDocs.Infrastructure.DomainServices;
using DNDocs.Infrastructure.UnitOfWork;
using DNDocs.Shared.Configuration;
using DNDocs.Domain.Entity;

namespace DNDocs.Infrastructure.Utils
{
    public static class RawRobiniaInfrastructure
    {
        static string __filesysPath = null;

        static string filesysPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(__filesysPath))
                    throw new InvalidOperationException("__filesyspath is not initialized");

                return __filesysPath;
            }
        }

        const string AppDbName = "app.sqlite";
        const string LogDbName = "log.sqlite";
        const string TempFilesFolderName = "temp";
        public static string TempFilesFolderFullPath => $"{filesysPath}/{TempFilesFolderName}";
        public static string OSPathAppDb => Path.Combine(filesysPath, AppDbName);
        public static string OSPathLogDb => Path.Combine(filesysPath, LogDbName);

        static int TempItemCounter = 0;

        internal static string CreateTempFolder()
        {
            int tempCounter = Interlocked.Increment(ref TempItemCounter);
            var datetime = DateTime.Now;
            var timestamp = datetime.ToString("yyyy_MM_dd_HH_mm_ss");
            string newTempFolderName = string.Format("{0}__{1}", timestamp, tempCounter);

            string tempFolderFullPath = Path.Combine(TempFilesFolderFullPath, newTempFolderName);

            Directory.CreateDirectory(tempFolderFullPath);

            return tempFolderFullPath;
        }


        internal static string AppDatabaseConnectionString() => string.Format("Data Source={0};", OSPathAppDb);

        public  static string LogDbConnectionString() => string.Format("Data Source={0};", OSPathLogDb);

        internal static string ConnectionStringForFile(string relativeFilePath)
        {
            return string.Format("Data Source={0};", Path.Combine(filesysPath, relativeFilePath));
        }

        public static void AddRobiniaInfrastructure(
            this IServiceCollection serviceCollection,
            string infrastructureRootDirectoryPath)
        {
            if (!Directory.Exists(infrastructureRootDirectoryPath))
                throw new Exception($"(Safety): Directory does not exists. Create this directory manually if intended to store files: '{infrastructureRootDirectoryPath}'");
            __filesysPath = infrastructureRootDirectoryPath;

            CreateInfrastructureFileSystem(infrastructureRootDirectoryPath);

            var dbConnectionString = AppDatabaseConnectionString();
            serviceCollection.AddDbContext<AppDbContext>(opt =>
                {
                    opt.UseSqlite(dbConnectionString);
                    opt.EnableDetailedErrors();
                    opt.EnableSensitiveDataLogging();
                });
            // serviceCollection.AddDbContext<TenantDbContext>((serviceProvider, builder) =>
            // {
            //     var tenantContext = serviceProvider.GetRequiredService<IAppContextInfo>().GetCurrentTenantContextOrThrow();
            // 
            // 
            //     builder.UseSqlite();
            // });

            serviceCollection.AddSingleton<IDNInfrastructure, RobiniaInfrastructure>();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddScoped<IGithubAPI, GithubAPI>();
            serviceCollection.AddSingleton<ICache, CacheService>();
            serviceCollection.AddHttpClient();
        }

        private static void CreateInfrastructureFileSystem(string infrastructureRootDirectoryPath)
        {
        }

        public static void Startup(DNDocsSettings settings)
        {

        }

        internal static void CreateSqliteDbIfNotExists(string connectionString)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                // Create database if not exists
                connection.Open();
                connection.Close();
            }
        }
        
    }
}