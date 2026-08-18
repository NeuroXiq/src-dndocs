
using DbUp;
using DNDocs.Job.Web.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace DNDocs.Job.Web.Infrastructure
{
    public interface IDJobInfrastructure
    {
        SqliteConnection OpenConnectionApp();
        void Startup();
    }

    public class DJobInfrastructure : IDJobInfrastructure
    {
        private string infrastructureDirectory;
        private string appConnectionString;

        public DJobInfrastructure(IOptions<DJobSettings> options)
        {
            infrastructureDirectory = options.Value.DataDirectory;

            if (!Directory.Exists(infrastructureDirectory))
                throw new Exception($"(Safety): directory does not exists: '{infrastructureDirectory}'. Create this directory manually");

            appConnectionString = $"Data Source={Path.Combine(infrastructureDirectory, "app.sqlite")};";
        }

        public void Startup()
        {
            RunAllMigrations();
        }

        private void RunAllMigrations()
        {
            RunMigration(appConnectionString, "DNDocs.Job.Web.Infrastructure.Migrations.App");
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
            var asm = typeof(DJobInfrastructure).Assembly;

            var upgrader = DeployChanges.To
                .SqliteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(asm, (resourceName) => resourceName.StartsWith(filter))
                .LogScriptOutput()
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw result.Error;
            }
        }

        public SqliteConnection OpenConnectionApp()
        {
            SqliteConnection connection = new SqliteConnection(appConnectionString);
            connection.Open();
            return connection;
        }
    }
}
