using BookCatalogApp.Core.Interfaces;
using BookCatalogApp.Core.Interfaces.Repositories;
using BookCatalogApp.Core.Services;
using BookCatalogApp.Data.Providers;
using BookCatalogApp.Data.Services;
using BookCatalogApp.Data.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookCatalogApp.Presentation.Services
{
    public static class ServiceConfiguration
    {
        private const string ConfigFileName = "appsettings.json";

        public static ServiceProvider Configure()
        {
            IConfiguration config = BuildConfiguration();

            ServiceCollection services = new ServiceCollection();

            ConfigureServices(services, config);

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            ApplyMigrations(serviceProvider);

            return serviceProvider;
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile(ConfigFileName)
                .Build();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<BookCatalogDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IBookRepository, BookRepository>();

            services.AddScoped<IBookService, BookService>();
            services.AddScoped<ICsvFileProvider, CsvFileProvider>();
            services.AddScoped<IJsonFileProvider, JsonFileProvider>();
        }

        private static void ApplyMigrations(ServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<BookCatalogDbContext>();
                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
                }

            }
        }
    }
}
