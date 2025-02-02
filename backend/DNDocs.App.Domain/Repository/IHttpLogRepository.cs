using DNDocs.Api.DTO.Admin;
using DNDocs.Domain.Entity;

namespace DNDocs.Domain.Repository
{
    public interface IHttpLogRepository : IRepository<HttpLog>
    {
        IList<HttpLog> TableDataLogs(TableDataRequest request);
        int UniqueIP(DateTime maxAge);
    }
}
