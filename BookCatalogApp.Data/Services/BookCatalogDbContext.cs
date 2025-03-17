using BookCatalogApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookCatalogApp.Data.Services
{
    public class BookCatalogDbContext : DbContext
    {
        public BookCatalogDbContext(DbContextOptions<BookCatalogDbContext> options)
            : base(options)
        { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Publisher> Publishers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasIndex(b => new { b.Title, b.AuthorId, b.PublisherId, b.ReleaseDate })
                .IsUnique();

            modelBuilder.Entity<Author>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<Genre>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<Publisher>()
                .HasIndex(p => p.Name)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
