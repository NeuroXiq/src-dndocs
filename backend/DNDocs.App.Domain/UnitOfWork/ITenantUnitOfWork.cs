using DNDocs.Domain.Repository;

namespace DNDocs.Domain.UnitOfWork
{
    public interface ITenantUnitOfWork
    {
        IKeyRepository<TEntity, int> GetSimpleRepository<TEntity>() where TEntity : Entity.Entity;
        void Commit();
    }
}
