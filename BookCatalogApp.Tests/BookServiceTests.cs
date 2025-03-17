using Moq;
using BookCatalogApp.Core.Interfaces;
using BookCatalogApp.Core.Services;
using BookCatalogApp.Models.Entities;
using BookCatalogApp.Models.Models;
using BookCatalogApp.Core.Interfaces.Repositories;

namespace BookCatalogApp.Tests
{
    [TestFixture]
    public class BookServiceTests
    {
        private BookService _bookService;
        private Mock<ICsvFileProvider> _mockCsvFileProvider;
        private Mock<IJsonFileProvider> _mockJsonFileProvider;
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<IRepository<Author>> _mockAuthorRepository;
        private Mock<IRepository<Genre>> _mockGenreRepository;
        private Mock<IRepository<Publisher>> _mockPublisherRepository;

        [SetUp]
        public void SetUp()
        {
            _mockCsvFileProvider = new Mock<ICsvFileProvider>();
            _mockJsonFileProvider = new Mock<IJsonFileProvider>();
            _mockBookRepository = new Mock<IBookRepository>();
            _mockAuthorRepository = new Mock<IRepository<Author>>();
            _mockGenreRepository = new Mock<IRepository<Genre>>();
            _mockPublisherRepository = new Mock<IRepository<Publisher>>();

            _bookService = new BookService(
                _mockBookRepository.Object,
                _mockGenreRepository.Object,
                _mockAuthorRepository.Object,
                _mockPublisherRepository.Object,
                _mockCsvFileProvider.Object,
                _mockJsonFileProvider.Object
            );
        }

        [Test]
        public async Task AddBooksFromFileAsync_ShouldAddBooksToDatabase()
        {
            IEnumerable<Book> books = GetSampleBooks();

            _mockGenreRepository.Setup(m => m.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, Genre>());

            _mockAuthorRepository.Setup(m => m.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, Author>());

            _mockPublisherRepository.Setup(m => m.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, Publisher>());

            _mockCsvFileProvider.Setup(m => m.ParseCsvFileAsync(It.IsAny<string>()))
                .ReturnsAsync(books);

            _mockBookRepository.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<Book>>()))
                .Returns(Task.CompletedTask);
            _mockBookRepository.Setup(m => m.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            string filePath = "example.csv";

            await _bookService.AddBooksFromFileAsync(filePath);

            _mockBookRepository.Verify(m => m.AddRangeAsync(It.Is<IEnumerable<Book>>(b => b.Count() == 2)), Times.Once);
            _mockBookRepository.Verify(m => m.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task AddBooksFromFileAsync_ShouldNotAddDuplicateBooksFromSameFile()
        {
            IEnumerable<Book> books = GetDuplicateSampleBooks();

            _mockGenreRepository.Setup(m => m.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, Genre>());

            _mockAuthorRepository.Setup(m => m.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, Author>());

            _mockPublisherRepository.Setup(m => m.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, Publisher>());

            _mockCsvFileProvider.Setup(m => m.ParseCsvFileAsync(It.IsAny<string>()))
                .ReturnsAsync(books);

            _mockBookRepository.Setup(m => m.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Book, bool>>>()))
                .ReturnsAsync(true);

            string filePath = "example.csv";

            await _bookService.AddBooksFromFileAsync(filePath);

            _mockBookRepository.Verify(m => m.AddRangeAsync(It.IsAny<IEnumerable<Book>>()), Times.Never);
        }

        [Test]
        public async Task SearchBooksAsync_ShouldWriteFilteredBooksToCsv()
        {
            IEnumerable<Book> books = GetSampleBooks();
            Filter filter = new Filter { Title = "Book One" };

            _mockJsonFileProvider.Setup(m => m.ReadFilterFromFileAsync(It.IsAny<string>()))
                .ReturnsAsync(filter);

            _mockBookRepository.Setup(m => m.GetBooksWithIncludesAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Book, bool>>>()))
                    .ReturnsAsync(books.Where(b => b.Title == "Book One"));

            _mockCsvFileProvider.Setup(m => m.WriteBooksToCsvAsync(It.IsAny<IEnumerable<Book>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            string outputDirectory = "C:\\TestOutput";

            await _bookService.SearchBooksAsync(outputDirectory);

            _mockCsvFileProvider.Verify(m => m.WriteBooksToCsvAsync(
                It.Is<IEnumerable<Book>>(b => b.Count() == 1 && b.First().Title == "Book One"),
                It.Is<string>(path => path.StartsWith(outputDirectory))), Times.Once);
        }

        private IEnumerable<Book> GetDuplicateSampleBooks()
        {
            return new List<Book>
            {
                new Book
                {
                    Title = "Book One",
                    Pages = 100,
                    ReleaseDate = new DateTime(2020, 1, 1),
                    Genre = new Genre { Name = "Fiction" },
                    Author = new Author { Name = "Author One" },
                    Publisher = new Publisher { Name = "Publisher One" }
                },
                new Book
                {
                    Title = "Book One",
                    Pages = 100,
                    ReleaseDate = new DateTime(2020, 1, 1),
                    Genre = new Genre { Name = "Fiction" },
                    Author = new Author { Name = "Author One" },
                    Publisher = new Publisher { Name = "Publisher One" }
                },
            };
        }

        private IEnumerable<Book> GetSampleBooks()
        {
            return new List<Book>
            {
                new Book
                {
                    Title = "Book One",
                    Pages = 100,
                    ReleaseDate = new DateTime(2020, 1, 1),
                    Genre = new Genre { Name = "Fiction" },
                    Author = new Author { Name = "Author One" },
                    Publisher = new Publisher { Name = "Publisher One" }
                },
                new Book
                {
                    Title = "Book Two",
                    Pages = 200,
                    ReleaseDate = new DateTime(2021, 1, 1),
                    Genre = new Genre { Name = "Non-Fiction" },
                    Author = new Author { Name = "Author Two" },
                    Publisher = new Publisher { Name = "Publisher Two" }
                }
            };
        }
    }
}