using DNDocs.Domain.ValueTypes;
using DNDocs.Api.DTO.Admin;
using System.Linq.Expressions;

namespace DNDocs.Domain.Repository
{
    public interface IRepository<TEntity> : IKeyRepository<TEntity, int> where TEntity : Entity.Entity
    {

    }

    public interface IKeyRepository<TEntity, TKey> : ICoreRepository<TEntity> where TEntity : class
    {
        void Create(TEntity entity);
        void Create(IEnumerable<TEntity> entity);
        TEntity GetById(TKey id);
        TEntity GetByIdChecked(TKey id);
        void Update(TEntity entity);
        void Delete(TKey id);
        void ExecuteDelete(Expression<Func<TEntity, bool>> predicate);
        void Delete(IEnumerable<int> ids);
        void Delete(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);

        IList<TEntity> GetAll();
        TableDataResponse<TEntity> GetTableData(TableDataRequest request);
        IQueryable<TEntity> Query(params Expression<Func<TEntity, object>>[] includes);
        IList<TEntity> GetByIdsChecked(IEnumerable<TKey> ids);

        Task CreateAsync(TEntity entity);
        Task CreateAsync(IEnumerable<TEntity> entity);
        Task<TEntity> GetByIdAsync(TKey id);
        Task<TEntity> GetByIdCheckedAsync(TKey id);
        Task DeleteAsync(TKey id);
        Task<int[]> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate);
        Task DeleteAsync(IEnumerable<int> ids);
        Task DeleteAsync(IEnumerable<TEntity> entities);
        Task<IList<TEntity>> GetAllAsync();
        Task<TableDataResponse<TEntity>> GetTableDataAsync(TableDataRequest request);
        Task<IList<TEntity>> GetByIdsCheckedAsync(IEnumerable<TKey> ids);
    }
}
