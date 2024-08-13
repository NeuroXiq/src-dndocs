using Dapper;
using DNDocs.Job.Web.Infrastructure;
using DNDocs.Job.Web.Model;
using DNDocs.Job.Web.ValueTypes;
using Microsoft.Data.Sqlite;
using Vinca.BufferLogger;

namespace DNDocs.Job.Web.Services
{
    public interface IDJobRepository
    {
        public Task InsertLogs(IEnumerable<LogRow> logs);
        public Task<BgJob> DequeueBgJob();
        Task<int> CountJobsWaiting();
        Task InsertJobAsync(BgJobState waiting, string data, DateTime utcNow);
        Task UpdateBgJobAsync(BgJob nextJob);
        Task<IEnumerable<BgJob>> SelectBgJobByState(BgJobState state);
    }

    public class DJobRepository : IDJobRepository
    {
        private IDJobInfrastructure infrastructure;

        public DJobRepository(IDJobInfrastructure infrastructure)
        {
            this.infrastructure = infrastructure;
        }

        public async Task<IEnumerable<BgJob>> SelectBgJobByState(BgJobState state)
        {
            using var con = infrastructure.OpenConnectionApp();
            return await con.QueryAsync<BgJob>($"{SqlSelectBgJob} WHERE state = @state", new { state });
        }

        public async Task InsertJobAsync(BgJobState state, string data, DateTime createdOn)
        {
            using var conn = infrastructure.OpenConnectionApp();
            var sql = 
                $"INSERT INTO bg_job (state, build_data, created_on, " + 
                "start_on, completed_on, exception, lock) " + 
                " VALUES (@state, @data, @createdOn, NULL, NULL, NULL, NULL)";
            await conn.ExecuteAsync(sql, new { state, data, createdOn });
        }

        public async Task UpdateBgJobAsync(BgJob job)
        {
            using var conn = infrastructure.OpenConnectionApp();
            var sql = $"UPDATE bg_job SET " +
                "state = @State, build_data = @BuildData, created_on = @CreatedOn, " +
                "start_on = @StartOn, completed_on = @CompletedOn, exception = @Exception, lock = @Lock " +
                "WHERE id = @Id";
            await conn.ExecuteAsync(sql, job);
        }

        public async Task<int> CountJobsWaiting()
        {
            using var conn = infrastructure.OpenConnectionApp();
            var sql = $"SELECT COUNT(*) FROM bg_job WHERE state = @Waiting";
            return await conn.ExecuteScalarAsync<int>(sql,new { BgJobState.Waiting });
        }

        public async Task<BgJob> DequeueBgJob()
        {
            // thread safe dequeue job

            string lockVal = Guid.NewGuid().ToString().ToUpper();

            using SqliteConnection conn = infrastructure.OpenConnectionApp();
            var sql = "UPDATE bg_job SET lock = @LockGuid " +
                "WHERE id = (" + 
                " SELECT id FROM bg_job " + 
                " WHERE state = @StateWaiting " + 
                " AND lock IS NULL " + 
                " ORDER BY created_on ASC LIMIT 1)";
            await conn.ExecuteAsync(sql, new { LockGuid = lockVal, StateWaiting = (int)BgJobState.Waiting });

            sql = $"{SqlSelectBgJob} WHERE lock = @LockGuid";

            return await conn.QueryFirstOrDefaultAsync<BgJob>(sql, new { LockGuid = lockVal });
        }

        public async Task InsertLogs(IEnumerable<LogRow> logs)
        {
            using SqliteConnection connection = infrastructure.OpenConnectionLog();
            var sql = "INSERT INTO app_log(message, category_name, log_level_id, date) " +
                "VALUES(@Message, @CategoryName, @LogLevel, @Date)";

            await connection.ExecuteAsync(sql, logs);
       }

        const string SqlSelectBgJob = "SELECT id as Id, [state] as State, build_data as BuildData, " +
                "created_on as CreatedOn, start_on as StartOn, completed_on as CompletedOn, " +
                "exception as Exception, lock as Lock " +
                "FROM bg_job";
    }
}
