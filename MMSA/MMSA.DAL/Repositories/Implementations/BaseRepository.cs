using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;
using System.Linq.Expressions;

namespace MMSA.DAL.Repositories.Implementations
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected AlgorithmDataContext _dataContext { get; set; }

        public BaseRepository(AlgorithmDataContext dataContext)
        {
            _dataContext = dataContext;
            _dataContext.Database.SetCommandTimeout(300);
        }

        public async Task<T> InsertAsync(T entity)
        {
            var result = await _dataContext.Set<T>().AddAsync(entity);

            return result.Entity;
        }

        public async Task<T> InsertAsync(T entity, bool? isSave = null)
        {
            var result = await _dataContext.Set<T>().AddAsync(entity);
            if (isSave != null && isSave.Value)
            {
                await SaveAsync();
            }

            return result.Entity;
        }

        public void Update(T entity)
        {
            _dataContext.Set<T>().Update(entity);
        }

        public async Task<T> UpdateAsync(T entity, bool? isSave = null)
        {
            _dataContext.Set<T>().Update(entity);
            if (isSave != null && isSave.Value)
            {
                await SaveAsync();
            }

            return entity;
        }

        public async Task<bool> DeleteAsync(T entity, bool? isSave = null)
        {
            _dataContext.Set<T>().Remove(entity);
            if (isSave != null && isSave.Value)
            {
                await SaveAsync();
            }

            return entity != null;
        }

        public void Attach(T entity)
        {
            _dataContext.Set<T>().Attach(entity);
        }

        public IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
        {
            IIncludableQueryable<T, object> query = null;

            if (includes.Length > 0)
            {
                query = _dataContext.Set<T>().Include(includes[0]);
            }
            for (int queryIndex = 1; queryIndex < includes.Length; ++queryIndex)
            {
                query = query.Include(includes[queryIndex]);
            }

            return query == null ? _dataContext.Set<T>() : query;
        }

        public async Task<IEnumerable<T>> GetItemListAsync(string select, string where, string group, string sort, string limit)
        {
            string query = $"{select} {where} {group} {sort} {limit}";
            return await _dataContext.Set<T>()
               .FromSqlRaw($"{select} {where} {group} {sort} {limit}")
               .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAndSortAllAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            var query = _dataContext.Set<T>().AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAsync(int skip, int take, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).Skip(skip).Take(take).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).ToListAsync();
        }

        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            var query = GetQuery(predicate, include);

            return await query.FirstAsync();
        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).FirstOrDefaultAsync();
        }

        public async Task<T> GetLastAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).LastAsync();
        }

        public async Task<T> GetLastOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).LastOrDefaultAsync();
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).SingleAsync();
        }

        public async Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            return await GetQuery(predicate, include).SingleOrDefaultAsync();
        }

        private IQueryable<T> GetQuery(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            var query = _dataContext.Set<T>().AsNoTracking();
            if (include != null)
            {
                query = include(query);
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query;
        }

        public async Task SaveAsync()
        {
            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) // Handles concurrency issues specifically
            {
                throw new Exception("A concurrency error occurred while saving changes. ", ex);
            }
            catch (DbUpdateException ex) // Handles database update issues specifically
            {
                throw new Exception($"An error occurred while updating the database. {ex?.InnerException?.Message}");
            }
            catch (Exception ex) // Handles all other exceptions
            {
                throw new Exception("An error occurred while saving changes. ", ex);
            }
        }

        public async Task<int> GetTotalCountAsync(Expression<Func<T, bool>> predicate = null)
        {
            return await GetQuery(predicate).CountAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _dataContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _dataContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _dataContext.Database.RollbackTransactionAsync();
        }
    }
}
