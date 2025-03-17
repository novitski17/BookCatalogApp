using System.Linq.Expressions;

namespace BookCatalogApp.Core.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetAllAsNoTracking();
        Task<Dictionary<string, T>> GetByNamesAsync(IEnumerable<string> names);
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate = null);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task SaveChangesAsync();
    }
}
