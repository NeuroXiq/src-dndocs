using Microsoft.Extensions.Logging;
using DNDocs.Shared.Log;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using DbUp;

namespace DNDocs.Infrastructure.Utils
{
    public interface IDNInfrastructure
    {
        void RunAppMigrations();
        string GetOSPathGitRepoStoreRepo(Guid repoid);
    }


    internal class RobiniaInfrastructure : IDNInfrastructure
    {
        private IServiceProvider serviceProvider;
        private ILogger<RobiniaInfrastructure> logger;

        public RobiniaInfrastructure(
            ILogger<RobiniaInfrastructure> logger,
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        static string FID(Guid guid)
        {
            if (Guid.Empty == guid) throw new ArgumentException("Empty guid not allowed to use in infrastucture (safe guard)");

            return guid.ToString().ToUpper();
        }

        public string GetOSPathGitRepoStoreRepo(Guid repoid) => Path.Combine(RawRobiniaInfrastructure.GitStoreFullPath, FID(repoid));

        public void RunAppMigrations()
        {
            RunMigrations(RawRobiniaInfrastructure.AppDatabaseConnectionString(), s => s.StartsWith("DNDocs.Infrastructure.Migrations."));
            RunMigrations(RawRobiniaInfrastructure.LogDbConnectionString(), s => s.StartsWith("DNDocs.Infrastructure.LogMigrations."));
        }

        public static void RunMigrations(string connectionString, Func<string, bool> filter)
        {
            RawRobiniaInfrastructure.CreateSqliteDbIfNotExists(connectionString);

            var asm = typeof(RawRobiniaInfrastructure).Assembly;
            string location = typeof(RawRobiniaInfrastructure).Assembly.Location;

            var upgrader = DeployChanges.To
                .SQLiteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(asm, filter)
                .LogScriptOutput()
                .LogTo(new log())
                .LogToConsole()
                //.WithTransaction() <-- needed by 0008 migration
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw result.Error;
            }
        }
    }
}
