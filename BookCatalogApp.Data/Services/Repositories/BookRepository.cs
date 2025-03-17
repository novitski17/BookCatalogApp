using BookCatalogApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BookCatalogApp.Core.Interfaces.Repositories;

namespace BookCatalogApp.Data.Services.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        private readonly BookCatalogDbContext _context;

        public BookRepository(BookCatalogDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetBooksWithIncludesAsync(Expression<Func<Book, bool>> predicate = null)
        {
            IQueryable<Book> query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.Publisher);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }
    }
}
