using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookCatalogApp.Data.Services
{
    public class BookCatalogDbContextFactory : IDesignTimeDbContextFactory<BookCatalogDbContext>
    {
        private const string ConfigFileName = "appsettings.json";

        public BookCatalogDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile(ConfigFileName)
                .Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<BookCatalogDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BookCatalogDbContext(optionsBuilder.Options);
        }
    }
}
