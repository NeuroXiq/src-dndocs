using DNDocs.Domain.Entity;

namespace DNDocs.Domain.Repository
{
    public interface IAppLogRepository : IRepository<AppLog>
    {
        IList<AppLog> GetLastLogs(int count, int minPriority);
    }
}
