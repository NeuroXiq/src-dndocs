using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Infrastructure.DataContext;
using DNDocs.Api.DTO.Admin;

namespace DNDocs.Infrastructure.Repository
{
    public class HttpLogRepository : BaseRepository<HttpLog>, IHttpLogRepository
    {
        public HttpLogRepository(AppDbContext dbcontext) : base(dbcontext) { }

        public IList<HttpLog> TableDataLogs(TableDataRequest request)
        {
            // filters
            // applog.where

            //
            
            var x =  dbset.OrderByDescending(e => e.DateTime)
                .Skip(request.Page * request.RowsPerPage)
                .Take(request.RowsPerPage)
                .ToList();

            return x;
        }

        public int UniqueIP(DateTime maxAge)
        {
            return 0;
            //var logs = dbset.Where(log => log.DateTime > maxAge)
            //    .OrderByDescending(t => t.DateTime)
            //    .Select(t => t.IP);

            //// shows warnings in logs if 'distinc' used with query above
            //return logs.Distinct().Count();
        }
    }
}
