namespace DNDocs.Domain.Repository
{
    public interface ICoreRepository<TEntity> where TEntity : class
    {
        void Create(TEntity entity);
    }
}
