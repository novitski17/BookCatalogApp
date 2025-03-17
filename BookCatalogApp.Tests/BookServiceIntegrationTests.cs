using BookCatalogApp.Core.Interfaces;
using BookCatalogApp.Core.Services;
using BookCatalogApp.Data.Providers;
using BookCatalogApp.Data.Services.Repositories;
using BookCatalogApp.Data.Services;
using BookCatalogApp.Models.Entities;
using BookCatalogApp.Models.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BookCatalogApp.Core.Interfaces.Repositories;

namespace BookCatalogApp.Tests
{
    [TestFixture]
    public class BookServiceIntegrationTests
    {
        private BookService _bookService;
        private BookCatalogDbContext _dbContext;
        private IBookRepository _bookRepository;
        private IRepository<Author> _authorRepository;
        private IRepository<Genre> _genreRepository;
        private IRepository<Publisher> _publisherRepository;
        private ICsvFileProvider _csvFileProvider;
        private IJsonFileProvider _jsonFileProvider;

        private string _csvContent;
        private string _duplicateCsvContent;
        private Filter _filter;

        private const string FilterFileName = "filter.json";
        private const string OutputFolder = "BookSearchHistory";
        private string _appBaseDirectory;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<BookCatalogDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new BookCatalogDbContext(options);

            _bookRepository = new BookRepository(_dbContext);
            _authorRepository = new Repository<Author>(_dbContext);
            _genreRepository = new Repository<Genre>(_dbContext);
            _publisherRepository = new Repository<Publisher>(_dbContext);

            _csvFileProvider = new CsvFileProvider();
            _jsonFileProvider = new JsonFileProvider();

            _bookService = new BookService(
                _bookRepository,
                _genreRepository,
                _authorRepository,
                _publisherRepository,
                _csvFileProvider,
                _jsonFileProvider);

            _csvContent = "Title,Pages,Genre,ReleaseDate,Author,Publisher\n" +
                          "Test Book,300,Test Genre,2020-01-01,Test Author,Test Publisher";

            _duplicateCsvContent = "Title,Pages,Genre,ReleaseDate,Author,Publisher\n" +
                                   "Test Book,300,Test Genre,2020-01-01,Test Author,Test Publisher\n" +
                                   "Test Book,300,Test Genre,2020-01-01,Test Author,Test Publisher";

            _filter = new Filter { Title = "Test Book" };

            _appBaseDirectory = AppContext.BaseDirectory;
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();

            string filterFilePath = Path.Combine(_appBaseDirectory, FilterFileName);
            if (File.Exists(filterFilePath))
            {
                File.Delete(filterFilePath);
            }

            string outputDirectory = Path.Combine(Path.GetTempPath(), "TestOutput");
            string fullOutputFolderPath = Path.Combine(outputDirectory, OutputFolder);
            if (Directory.Exists(fullOutputFolderPath))
            {
                Directory.Delete(fullOutputFolderPath, true);
            }
        }

        [Test]
        public async Task AddBooksFromFileAsync_ShouldAddBooksToDatabase()
        {
            using (var tempFile = CreateTempCsvFile(_csvContent))
            {
                await _bookService.AddBooksFromFileAsync(tempFile.FilePath);

                var booksInDb = await _bookRepository.GetAll().Include(b => b.Author).ToListAsync();
                Assert.AreEqual(1, booksInDb.Count);
                Assert.AreEqual("Test Book", booksInDb[0].Title);
            }
        }

        [Test]
        public async Task ShouldNotAddDuplicateBooksFromSameFile()
        {
            using (var tempFile = CreateTempCsvFile(_duplicateCsvContent))
            {
                await _bookService.AddBooksFromFileAsync(tempFile.FilePath);

                var booksInDb = await _bookRepository.GetAll().Include(b => b.Author).ToListAsync();
                Assert.AreEqual(1, booksInDb.Count);
                Assert.AreEqual("Test Book", booksInDb[0].Title);
            }
        }

        [Test]
        public async Task ShouldNotAddDuplicateBooksFromDifferentFiles()
        {
            using (var tempFile1 = CreateTempCsvFile(_csvContent))
            using (var tempFile2 = CreateTempCsvFile(_csvContent))
            {
                await _bookService.AddBooksFromFileAsync(tempFile1.FilePath);
                await _bookService.AddBooksFromFileAsync(tempFile2.FilePath);

                var booksInDb = await _bookRepository.GetAll().Include(b => b.Author).ToListAsync();
                Assert.AreEqual(1, booksInDb.Count);
                Assert.AreEqual("Test Book", booksInDb[0].Title);
            }
        }

        [Test]
        public async Task SearchBooksAsync_ShouldReturnCorrectBooks()
        {
            await SeedDatabaseAsync();

            string filterFilePath = Path.Combine(_appBaseDirectory, FilterFileName);
            File.WriteAllText(filterFilePath, JsonConvert.SerializeObject(_filter));

            string outputDirectory = Path.Combine(Path.GetTempPath(), "TestOutput");

            await _bookService.SearchBooksAsync(outputDirectory);

            string fullOutputFolderPath = Path.Combine(outputDirectory, OutputFolder);
            string[] files = Directory.GetFiles(fullOutputFolderPath, "*.csv", SearchOption.AllDirectories);
            Assert.IsTrue(files.Length > 0);

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        private TempFile CreateTempCsvFile(string content)
        {
            return new TempFile(content);
        }

        private async Task SeedDatabaseAsync()
        {
            var author = new Author { Name = "Test Author" };
            var genre = new Genre { Name = "Test Genre" };
            var publisher = new Publisher { Name = "Test Publisher" };

            var book = new Book
            {
                Title = "Test Book",
                Pages = 300,
                ReleaseDate = new DateTime(2020, 1, 1),
                Author = author,
                Genre = genre,
                Publisher = publisher
            };

            await _bookRepository.AddAsync(book);
            await _bookRepository.SaveChangesAsync();
        }

        private class TempFile : IDisposable
        {
            public string FilePath { get; private set; }

            public TempFile(string content)
            {
                FilePath = Path.GetTempFileName();
                File.WriteAllText(FilePath, content);
            }

            public void Dispose()
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
        }
    }
}
