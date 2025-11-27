using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }
        public async Task<TEntity?> GetByNameAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == id, cancellationToken);
        }
        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<TResult?> GetByIdWithIncludesAsync<TResult>(
    Expression<Func<TEntity, bool>> predicate,
    Expression<Func<TEntity, TResult>>? selector = null,
    CancellationToken cancellationToken = default,
    params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet.AsNoTracking();

            foreach (var include in includes)
                query = query.Include(include);

            if (selector is not null)
            {
                return await query
                    .Where(predicate)
                    .Select(selector)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            // إذا لم يوجد selector وأردنا إرجاع TEntity مباشرة
            var entity = await query
                .Where(predicate)
                .FirstOrDefaultAsync(cancellationToken);

            return (TResult?)(object?)entity; // cast إلى TResult
        }
        public async Task<List<TEntity>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => ids.Contains(EF.Property<Guid>(e, "Id")))
                .ToListAsync(cancellationToken);
        }
        public async Task<List<TEntity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => ids.Contains(EF.Property<int>(e, "Id")))
                .ToListAsync(cancellationToken);
        }
        public async Task<List<TResult>> FindAllWithIncludesAsync<TResult>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TResult>>? selector = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet.AsNoTracking();

            foreach (var include in includes)
                query = query.Include(include);

            if (selector is not null)
            {
                return await query
                    .Where(predicate)
                    .Select(selector)
                    .ToListAsync(cancellationToken);
            }

            // إذا لم يوجد selector وأردنا إرجاع TEntity مباشرة
            var entities = await query
                .Where(predicate)
                .ToListAsync(cancellationToken);

            return entities.Cast<TResult>().ToList(); // cast إلى TResult
        }
        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }
        public async Task<IReadOnlyList<TEntity>> GetListAsync(
       Expression<Func<TEntity, bool>> predicate,
       CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        // public async Task<TResult?> GetByIdWithIncludesAsync<TResult>(
        //Expression<Func<TEntity, bool>> predicate,
        //Expression<Func<TEntity, TResult>> selector,
        //CancellationToken cancellationToken = default,
        //params Expression<Func<TEntity, object>>[] includes)
        // {
        //     IQueryable<TEntity> query = _dbSet;

        //     foreach (var include in includes)
        //         query = query.Include(include);

        //     return await query
        //         .Where(predicate)
        //         .Select(selector)
        //         .FirstOrDefaultAsync(cancellationToken);
        // }

        // public async Task<List<TResult>> FindAllWithIncludesAsync<TResult>(
        //     Expression<Func<TEntity, bool>> predicate,
        //     Expression<Func<TEntity, TResult>> selector,
        //     CancellationToken cancellationToken = default,
        //     params Expression<Func<TEntity, object>>[] includes)
        // {
        //     IQueryable<TEntity> query = _dbSet;

        //     foreach (var include in includes)
        //         query = query.Include(include);

        //     return await query
        //         .Where(predicate)
        //         .Select(selector)
        //         .ToListAsync(cancellationToken);
        // }



        public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        public IQueryable<TEntity> Query()
        {
            return _dbSet.AsQueryable();
        }



    }

}
