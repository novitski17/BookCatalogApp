using BookCatalogApp.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCatalogApp.Data.Services.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly BookCatalogDbContext _context;
        private readonly DbSet<T> _dbSet;

        private const string NameProperty = "Name";

        public Repository(BookCatalogDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        IQueryable<T> IRepository<T>.GetAllAsNoTracking()
        {
            return _dbSet.AsNoTracking();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        public async Task<Dictionary<string, T>> GetByNamesAsync(IEnumerable<string> names)
        {
            return await _dbSet
                .Where(e => names.Contains(EF.Property<string>(e, NameProperty)))
                .ToDictionaryAsync(e => (string)typeof(T).GetProperty(NameProperty).GetValue(e));
        }

        async Task<IEnumerable<T>> IRepository<T>.GetAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate != null)
                return await _dbSet.Where(predicate).ToListAsync();

            return await _dbSet.ToListAsync();
        }

        async Task IRepository<T>.AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        async Task IRepository<T>.AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        void IRepository<T>.Update(T entity)
        {
            _dbSet.Update(entity);
        }

        void IRepository<T>.Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        async Task<bool> IRepository<T>.ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        async Task IRepository<T>.SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
