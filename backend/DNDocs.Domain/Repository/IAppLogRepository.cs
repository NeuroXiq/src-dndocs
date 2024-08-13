using DNDocs.Domain.Entity.App;

namespace DNDocs.Domain.Repository
{
    public interface IAppLogRepository : IRepository<AppLog>
    {
        IList<AppLog> GetLastLogs(int count, int minPriority);
    }
}
