using DNDocs.Domain.Entity;
using DNDocs.Domain.Repository;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Infrastructure.DataContext;
using DNDocs.Infrastructure.Repository;

namespace DNDocs.Infrastructure.UnitOfWork
{
    public class AppUnitOfWork : IAppUnitOfWork
    {
        private AppDbContext dbctx;

        public IBgJobRepository BgJobRepository => new BgJobRepository(dbctx);

        public IUserRepository UserRepository => new UserRepository(dbctx);

        public INugetOrgProjectRepository NugetOrgProjectRepository => new NugetOrgProjectRepository(dbctx);

        public AppUnitOfWork(AppDbContext dbctx)
        {
            this.dbctx = dbctx;
        }

        public IRepository<TEntity> GetSimpleRepository<TEntity>() where TEntity : EntityBase
        {
            return new BaseRepository<TEntity>(dbctx);
        }

        public void SaveChanges()
        {
            dbctx.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await dbctx.SaveChangesAsync();
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : EntityBase
        {
            return dbctx.Set<TEntity>();
        }
    }
}
