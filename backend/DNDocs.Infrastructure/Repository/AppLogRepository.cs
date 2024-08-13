using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Infrastructure.DataContext;
using System.Linq.Expressions;

namespace DNDocs.Infrastructure.Repository
{
    public class AppLogRepository : BaseRepository<AppLog>, IAppLogRepository
    {
        public AppLogRepository(AppDbContext dbcontext) : base(dbcontext) { }

        public IList<AppLog> GetLastLogs(int count, int minPriority)
        {
            return dbset.Where(t => t.LogLevelId >= minPriority)
                .OrderByDescending(t => t.Date)
                .Take(count)
                .ToList();
        }

        private static IQueryable<T> FilterDynamic<T>(IQueryable<T> query, string fieldname, object value)
        {
            var param = Expression.Parameter(typeof(T), "e");
            var prop = Expression.PropertyOrField(param, fieldname);
            var body = Expression.Call(typeof(Enumerable), "Contains", new[] { value.GetType() }, Expression.Constant(value), prop);

            var predicate = Expression.Lambda<Func<T, bool>>(body, param);

            return query.Where(predicate);
        }
    }
}
