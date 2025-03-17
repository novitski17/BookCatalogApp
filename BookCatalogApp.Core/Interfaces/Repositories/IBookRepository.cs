using BookCatalogApp.Models.Entities;
using System.Linq.Expressions;

namespace BookCatalogApp.Core.Interfaces.Repositories
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksWithIncludesAsync(Expression<Func<Book, bool>> predicate = null);
    }
}
