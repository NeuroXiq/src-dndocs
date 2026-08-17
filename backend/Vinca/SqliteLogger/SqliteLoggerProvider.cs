using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.Versioning;

namespace Vinca.SqliteLogger
{
    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("BufferLogger")]
    public sealed class SqliteLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ConcurrentDictionary<string, SqliteLogger> loggers;
        private readonly Timer saveLogsTimer;
        private readonly Timer clearLogsTimer;
        private readonly ConcurrentQueue<LogRow> logsQueue;
        private readonly IDisposable onChangeToken;
        private SqliteLoggerOptions options;

        private volatile bool lockSavingLogs;

        public SqliteLoggerProvider(
            IServiceProvider serviceProvider,
            IOptionsMonitor<SqliteLoggerOptions> options)
        {
            this.serviceProvider = serviceProvider;
            this.options = options.CurrentValue;
            onChangeToken = options.OnChange(updated => this.options = updated);
            loggers = new ConcurrentDictionary<string, SqliteLogger>();
            lockSavingLogs = false;
            logsQueue = new ConcurrentQueue<LogRow>();
            saveLogsTimer = new Timer(OnTickSaveLogsInSqliteDb, null, TimeSpan.FromSeconds(1), this.options.SaveTimerPeriod);
            clearLogsTimer = new Timer(OnTickClearLogs, null, TimeSpan.FromSeconds(1), this.options.ClearTimerPeriod);
        }

        public static string CreateSqliteConnectionString(string filePath)
        {
            return $"Data Source={filePath};";
        }

        private void OnTickClearLogs(object state)
        {
            Task.Run(() =>
            {
                try
                {
                    int affected = 1;

                    while (affected > 0)
                    {
                        string retention = DateTimeOffset.Now.Subtract(options.RetentionPeriod).ToString("O");
                        using var sqliteConnection = new SqliteConnection(CreateSqliteConnectionString(options.SqliteDbPath));
                        sqliteConnection.Open();
                        using var command = sqliteConnection.CreateCommand();
                        command.CommandText = $"DELETE FROM app_log WHERE id IN (SELECT id FROM app_log WHERE  date < '{retention}' LIMIT 1000)";
                        affected = command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("failed to delete logs", e);
                }
            });
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, name => new SqliteLogger(name, logsQueue));
        }

        public void Dispose()
        {
            loggers.Clear();
            onChangeToken?.Dispose();
        }

        private void OnTickSaveLogsInSqliteDb(object state)
        {
            if (Interlocked.Exchange(ref lockSavingLogs, true)) return;

            Task.Run(async () =>
            {
                try
                {
                    DoSaveLogs();
                }
                catch (Exception e)
                {
                    Console.WriteLine("failed to save logs", e);
                }
                finally
                {
                    lockSavingLogs = false;
                }
            });
        }

        private void DoSaveLogs()
        {
            List<LogRow> logs = new List<LogRow>();

            for (int i = 0; i < 10000 && logsQueue.TryDequeue(out var log); i++)
            {
                logs.Add(log);
            }

            // var allLoggers = this.olo
            string connectionString = CreateSqliteConnectionString(options.SqliteDbPath);
            using var sqliteConnection = new SqliteConnection(connectionString);
            sqliteConnection.Open();

            using var command = sqliteConnection.CreateCommand();
            using var transaction = sqliteConnection.BeginTransaction();

            command.Transaction = transaction;
            command.CommandType = System.Data.CommandType.Text;

            foreach (var log in logs)
            {
                // that should: 1. command.prepare();  2. use sql parameters, this need that fix
                var msg = log.Message == null ? "NULL" : $"'{log.Message.Replace("'", "''")}'";
                command.CommandText =
                "INSERT INTO app_log(message, category_name, log_level_id, event_id, event_name, [date]) " +
                $"VALUES ({msg}, '{log.CategoryName}', {(int)log.LogLevel}, {log.EventId.Id}, '{log.EventId.Name}', '{log.Date.ToString("O")}')";

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public static class SqliteLoggerExtensions
    {
        const string OptionsName = $"Vinca:SqliteLoggerOptions";

        public static void AddVSqliteLogger(this WebApplicationBuilder builder, Action<SqliteLoggerOptions> configue = null)
        {
            var optionsBuilder= builder.Services.AddOptions<SqliteLoggerOptions>().Bind(builder.Configuration.GetSection(OptionsName));

            optionsBuilder
                .Validate(o =>
                    !string.IsNullOrWhiteSpace(o.SqliteDbPath) && Directory.Exists(Path.GetDirectoryName(o.SqliteDbPath)),
                    $"{OptionsName}.SqliteDbPath - empty or directory not not exists")
                .Validate(o =>
                    o.SaveTimerPeriod > TimeSpan.FromMilliseconds(100),
                    $"{OptionsName}.SaveTimerPeriod - less that 100ms (if needed remove that validation?)")
                .Validate(o =>
                    o.ClearTimerPeriod > TimeSpan.FromMilliseconds(100),
                    $"{OptionsName}.ClearTimerPeriod - less that 100ms (if needed remove that validation?)")
                .Validate(o =>
                    o.RetentionPeriod > TimeSpan.FromMilliseconds(100),
                    $"{OptionsName}.RetentionPeriod - less that 100ms (if needed remove that validation?)")
                .ValidateOnStart();

            string dbPath = builder.Configuration.GetSection($"{OptionsName}:SqliteDbPath").Get<string>();

            CreateLogsDb(dbPath);
            
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SqliteLoggerProvider>());
        }

        static void CreateLogsDb(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath)) throw new ArgumentException("sqlite path is empty");
            if (!Directory.Exists(Path.GetDirectoryName(dbPath))) throw new ArgumentException($"directory not exists: '{dbPath}'");
            if (File.Exists(dbPath)) return;

            string createText = """

PRAGMA journal_mode=WAL;
PRAGMA page_size=4096;
BEGIN TRANSACTION;

create table app_log
(
id integer primary key autoincrement,
[message] text,
category_name text,
log_level_id int,
event_id int,
event_name text,
[date] text
);
COMMIT;
""";

            Console.WriteLine("SqliteLogger: Db sqlite file not exists, creating new database");

            using var connection = new SqliteConnection(SqliteLoggerProvider.CreateSqliteConnectionString(dbPath));
            connection.Open();

            using var command = new SqliteCommand(createText, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
