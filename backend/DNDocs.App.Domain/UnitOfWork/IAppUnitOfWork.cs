using DNDocs.Domain.Repository;

namespace DNDocs.Domain.UnitOfWork
{
    public interface IAppUnitOfWork
    {
        IProjectRepository ProjectRepository { get; }
        IBgJobRepository BgJobRepository { get; }
        IUserRepository UserRepository { get; }

        IRepository<TEntity> GetSimpleRepository<TEntity>() where TEntity : Entity.EntityBase;
        IQueryable<TEntity> Query<TEntity>() where TEntity : Entity.EntityBase;
        void SaveChanges();
        Task SaveChangesAsync();
    }
}
