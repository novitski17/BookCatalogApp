using Moq;
using BookCatalogApp.Core.Models;
using BookCatalogApp.Models.Entities;
using BookCatalogApp.Data.Providers;
using BookCatalogApp.Data.Services.Validators;

namespace BookCatalogApp.Tests
{
    [TestFixture]
    public class CsvFileProviderTests
    {
        private CsvFileProvider _csvFileProvider;
        private Mock<CsvRecordValidator> _validatorMock;
        private const string TestFilePath = "example.csv";

        private const string ValidCsvData =
            "Title,Pages,Genre,ReleaseDate,Author,Publisher\n" +
            "To Kill a Mockingbird1,336,Fiction,1960-07-11,Harper Lee,HarperCollins1\n";

        private const string InvalidCsvData =
            "Title,Pages,Genre,ReleaseDate,Author,Publisher\n" +
            "InvalidBook,,Fiction,InvalidDate,,HarperCollins1\n";

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<CsvRecordValidator>();
            _csvFileProvider = new CsvFileProvider();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(TestFilePath))
            {
                File.Delete(TestFilePath);
            }
        }

        [Test]
        public async Task ParseCsvFileAsync_ValidCsv_ShouldReturnBooks()
        {
            File.WriteAllText(TestFilePath, ValidCsvData);

            var result = await _csvFileProvider.ParseCsvFileAsync(TestFilePath);

            Assert.IsNotNull(result);
            Book book = result.First();
            Assert.AreEqual("To Kill a Mockingbird1", book.Title);
            Assert.AreEqual(336, book.Pages);
            Assert.AreEqual("Fiction", book.Genre.Name);
            Assert.AreEqual("Harper Lee", book.Author.Name);
            Assert.AreEqual("HarperCollins1", book.Publisher.Name);
        }

        [Test]
        public async Task ParseCsvFileAsync_InvalidCsv_ShouldThrowException()
        {
            File.WriteAllText(TestFilePath, InvalidCsvData);

            Exception ex = Assert.ThrowsAsync<Exception>(async () =>
                await _csvFileProvider.ParseCsvFileAsync(TestFilePath));

            Assert.That(ex.Message, Does.StartWith($"Error parsing CSV file '{TestFilePath}':"));
        }

        [Test]
        public async Task WriteBooksToCsvAsync_ShouldWriteBooksToFile()
        {
            var books = new List<Book>
            {
                new Book
                {
                    Title = "To Kill a Mockingbird1",
                    Pages = 336,
                    ReleaseDate = new System.DateTime(1960, 07, 11),
                    Genre = new Genre { Name = "Fiction" },
                    Author = new Author { Name = "Harper Lee" },
                    Publisher = new Publisher { Name = "HarperCollins1" }
                }
            };

            await _csvFileProvider.WriteBooksToCsvAsync(books, TestFilePath);

            var fileContent = await File.ReadAllTextAsync(TestFilePath);
            Assert.IsTrue(fileContent.Contains("To Kill a Mockingbird1"));
            Assert.IsTrue(fileContent.Contains("336"));
            Assert.IsTrue(fileContent.Contains("Fiction"));
            Assert.IsTrue(fileContent.Contains("1960-07-11"));
            Assert.IsTrue(fileContent.Contains("Harper Lee"));
            Assert.IsTrue(fileContent.Contains("HarperCollins1"));
        }

        [Test]
        public void CsvRecordValidator_ShouldReturnErrorsForInvalidRecords()
        {
            var record = new BookCsv
            {
                Title = "",
                Pages = -1,
                Genre = "",
                ReleaseDate = "InvalidDate",
                Author = "",
                Publisher = ""
            };

            List<string> errors = _validatorMock.Object.Validate(record);

            Assert.AreEqual(6, errors.Count);
            Assert.Contains("Title is required.", errors);
            Assert.Contains("Pages must be a positive integer.", errors);
            Assert.Contains("Author is required.", errors);
            Assert.Contains("Genre is required.", errors);
            Assert.Contains("Publisher is required.", errors);
            Assert.Contains("Invalid date format for ReleaseDate: InvalidDate", errors);
        }
    }
}
