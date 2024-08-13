using Microsoft.EntityFrameworkCore;
using DNDocs.Domain.Entity;
using DNDocs.Domain.Repository;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;
using DNDocs.Api.DTO.Admin;
using System.Linq.Expressions;
using System.Text.Json;

namespace DNDocs.Infrastructure.Repository
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        protected DbContext dbcontext;
        protected DbSet<TEntity> dbset;

        public BaseRepository(DbContext dbcontext)
        {
            this.dbcontext = dbcontext ?? throw new ArgumentNullException(nameof(dbcontext));
            this.dbset = dbcontext.Set<TEntity>();
        }

        #region Sync

        public IQueryable<TEntity> Query(params Expression<Func<TEntity, object>>[] includes)
        {
            var queryable = dbset.AsQueryable();

            if (includes != null)
            {
                queryable = includes.Aggregate(queryable, (current, include) => current.Include(include));
            }

            return queryable;
        }

        public TEntity GetByIdChecked(int id)
        {
            return GetByIdCheckedAsync(id).Result;
        }

        // public IList<TEntity> Query(Func<TEntity, bool> predicate)
        // {
        //     return dbset.Where(predicate).ToList();
        // }

        public void Create(IEnumerable<TEntity> entity)
        {
            dbset.AddRange(entity);
        }

        public void Create(TEntity entity)
        {
            dbset.Add(entity);
        }

        public void Delete(int id)
        {
            var entity = dbcontext.Set<TEntity>().Where(t => t.Id == id).FirstOrDefault();

            if (entity == null) throw new BusinessLogicException("CannotDeleteEntityBecauseDoesNotExist");

            dbset.Remove(entity);
        }

        public void Delete(TEntity entity)
        {
            Validation.AppEx(entity == null, $"entity is null: {typeof(TEntity).FullName}");

            dbset.Remove(entity);
        }

        public void Delete(IEnumerable<int> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));
            if (!ids.Any()) return;

            var todelChunks = ids.Chunk(50);

            foreach (var toDeleteIds in todelChunks)
            {
                if (!toDeleteIds.Any()) continue;
                dbset.Where(t => toDeleteIds.Contains(t.Id)).ExecuteDelete();
            }
        }

        public void Delete(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            if (!entities.Any()) return;

            dbset.RemoveRange(entities);
        }

        public TEntity GetById(int id)
        {
            return dbcontext.Set<TEntity>().Where(e => e.Id == id).FirstOrDefault();
        }

        public void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> GetAll()
        {
            return dbcontext.Set<TEntity>().ToList();
        }

        public void ExecuteDelete(Expression<Func<TEntity, bool>> predicate)
        {
            var set = dbcontext.Set<TEntity>();

            set.Where(predicate).ExecuteDelete();
        }

        public TableDataResponse<TEntity> GetTableData(TableDataRequest request)
        {
            //TODO rename this method to 'GetTAbleData' and remove method above. test if this work
            var tableName = dbcontext.Model.FindEntityType(typeof(TEntity)).GetTableName();
            var fsql = ApplyFilters(request.Filters);
            var orderBy = GenerateOrderBySql(request.OrderBy);
            string sql = $"SELECT * FROM {tableName} t ";

            if (!string.IsNullOrEmpty(fsql)) sql += $" WHERE ({fsql}) ";
            if (!string.IsNullOrWhiteSpace(orderBy)) sql += orderBy;


            var query = dbset.FromSqlRaw(sql);

            var rows = query
                .Skip((request.Page) * request.RowsPerPage)
                .Take(request.RowsPerPage)
                .ToList();

            int count = -1;

            if (request.GetRowsCount)
            {
                count = query.Count();
            }

            return new TableDataResponse<TEntity>(count, request.Page, request.RowsPerPage, rows);
        }

        private string GenerateOrderBySql(IList<TableDataRequest.OrderByInfo> orderBy)
        {
            if (orderBy == null || orderBy.Count == 0) return null;
            string sql = "ORDER BY ";

            foreach (var ob in orderBy)
            {
                var sqlcol = SqlColName(ob.Column);
                var dir = ob.Dir == TableDataRequest.OrderDir.Asc ? "ASC" : "DESC";

                sql += $"{sqlcol} {dir},";
            }

            sql = sql.TrimEnd(',');

            return sql;
        }

        private string SqlColName(string propertyName)
        {
            //var colname = dbcontext.Model.FindEntityType(typeof(TEntity)).GetProperty(propertyName).GetColumnName();
            var et = dbcontext.Model.FindEntityType(typeof(TEntity));
            var prop = et.GetProperties().FirstOrDefault(t => t.Name.ToLower() == propertyName.ToLower());

            if (prop == null)
                throw new RobiniaException($"Failed to find column '{propertyName}'");

            var colname = prop.GetColumnName();

            return colname;
        }

        private string ApplyFilters(List<TableDataRequest.Filter> filters)
        {
            List<string> sqls = new List<string>();

            foreach (var f in filters)
            {

                var colname = SqlColName(f.Column);

                colname = $"t.{colname}";
                if (string.IsNullOrWhiteSpace(f.Value)) continue;

                /*<option value="like">like</option>
                        <option value="notlike">Not Like</option>
                        <option value="eq">Equal</option>
                        <option value="neq">Not Equal</option>
                        <option value="gte">Greated or equal</option>
                        <option value="lte">Less or equal</option>*/

                if (f.Type == "in")
                {
                    string[] vals;

                    try { vals = JsonSerializer.Deserialize<string[]>(f.Value); }
                    catch (Exception e)
                    { throw new Exception("Invalid 'value' field for a filter. For 'in' value field must be a JSON array", e); }

                    if (vals.Length == 0) continue;

                    var cvals = vals.Select(t => $"'{t}'");

                    sqls.Add($"{colname} IN ({string.Join(",", cvals)})");
                }
                else if (f.Type == "like")
                {
                    if (string.IsNullOrWhiteSpace(f.Value)) continue;

                    sqls.Add($"{colname} LIKE '%{f.Value}%'");
                }
                else if (f.Type == "notlike")
                {
                    if (string.IsNullOrWhiteSpace(f.Value)) continue;

                    sqls.Add($"{colname} NOT LIKE '%{f.Value}%'");
                }
                else if (f.Type == "eq")
                {
                    sqls.Add($"{colname} = '{f.Value}'");
                }
                else if (f.Type == "neq")
                {
                    sqls.Add($"{colname} <> '{f.Value}'");
                }
                else if (f.Type == "gte")
                {
                    sqls.Add($"{colname} >= '{f.Value}'");
                }
                else if (f.Type == "lte")
                {
                    sqls.Add($"{colname} <= '{f.Value}'");
                }
            }

            var resarr = sqls.Select(a => $"({a})").ToArray();
            var res = string.Join(" AND ", resarr);

            return res;
        }

        public IList<TEntity> GetByIdsChecked(IEnumerable<int> ids)
        {
            return GetByIdsCheckedAsync(ids).Result;
        }

        #endregion

        #region async

        public async Task CreateAsync(TEntity entity)
        {
            await this.dbcontext.Set<TEntity>().AddAsync(entity);
        }

        public async Task CreateAsync(IEnumerable<TEntity> entity)
        {
            await this.dbcontext.Set<TEntity>().AddRangeAsync(entity);
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await dbset.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TEntity> GetByIdCheckedAsync(int id)
        {
            var result = await dbset.Where(t => t.Id == id).ToListAsync();
            if (result.Count == 0) throw new EntityNotFoundException<TEntity>(id);
            if (result.Count > 1)
            {
                throw new RobiniaException($"SingleById: Expected Single entity by id but found more than one: found count: {result.Count}");
            }

            return result[0];
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteDeleteAsync(t => t.Id == id);
        }

        public async Task<int[]> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var deleted = await dbset.Where(predicate).Select(t => t.Id).ToArrayAsync();
            await dbset.Where(predicate).ExecuteDeleteAsync();

            return deleted;
        }

        public async Task DeleteAsync(IEnumerable<int> ids)
        {
            var idsArray = ids.ToArray();

            await ExecuteDeleteAsync(t => idsArray.Contains(t.Id));
        }

        public async Task DeleteAsync(IEnumerable<TEntity> entities)
        {
            dbset.RemoveRange(entities);
            return;
        }

        public async Task<IList<TEntity>> GetAllAsync()
        {
            return await dbset.Where(t => true).ToListAsync();
        }

        public Task<TableDataResponse<TEntity>> GetTableDataAsync(TableDataRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<TEntity>> GetByIdsCheckedAsync(IEnumerable<int> ids)
        {
            Validation.ArgNotNull(ids, nameof(ids));

            if (!ids.Any()) return new List<TEntity>();

            var idsArray = ids.ToArray();

            var result = await dbset.Where(t => idsArray.Contains(t.Id)).ToListAsync();

            Validation.AppEx(result.Count != idsArray.Count(), "Count of result does not match ids count");

            foreach (var id in ids)
            {
                Validation.AppEx(result.Any(r => r.Id == id), $"Expected entity with id: '{id}' but did not found in database results");
            }

            return result;
        }


        #endregion


    }
}
