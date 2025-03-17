using Microsoft.Extensions.Logging;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using DbUp;

namespace DNDocs.Infrastructure.Utils
{
    public interface IDNInfrastructure
    {
        void RunAppMigrations();
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

        public void RunAppMigrations()
        {
            RunMigrations(RawRobiniaInfrastructure.AppDatabaseConnectionString(), s => s.StartsWith("DNDocs.App.Infrastructure.Migrations."));
            RunMigrations(RawRobiniaInfrastructure.LogDbConnectionString(), s => s.StartsWith("DNDocs.App.Infrastructure.LogMigrations."));
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
