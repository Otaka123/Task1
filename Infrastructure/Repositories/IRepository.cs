using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        IQueryable<TEntity> Query();

    }
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

        Task<TEntity?> GetByNameAsync(string id, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        //Task<TEntity?> GetByIdAsync(TKey id, bool includeDeleted, CancellationToken cancellationToken = default);
        Task<TResult?> GetByIdWithIncludesAsync<TResult>(
          Expression<Func<TEntity, bool>> predicate,
          Expression<Func<TEntity, TResult>>? selector = null,
          CancellationToken cancellationToken = default,
          params Expression<Func<TEntity, object>>[] includes);
        Task<List<TResult>> FindAllWithIncludesAsync<TResult>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>>? selector = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes);

        Task<IReadOnlyList<TEntity>> GetListAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

        IQueryable<TEntity> Query();

    }
}
