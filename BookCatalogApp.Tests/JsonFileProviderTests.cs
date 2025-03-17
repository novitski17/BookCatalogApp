using BookCatalogApp.Data.Providers;
using BookCatalogApp.Models.Models;
using Newtonsoft.Json;

namespace BookCatalogApp.Tests
{
    [TestFixture]
    public class JsonFileProviderTests
    {
        private JsonFileProvider _jsonFileProvider;
        private const string TestFilePath = "example.json";

        [SetUp]
        public void SetUp()
        {
            _jsonFileProvider = new JsonFileProvider();
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
        public async Task ReadFilterFromFileAsync_ValidJson_ShouldReturnFilter()
        {
            Filter filter = new Filter
            {
                Title = "Test Title",
                Author = "Test Author",
                Genre = "Fiction",
                MoreThanPages = 100
            };
            string jsonContent = JsonConvert.SerializeObject(filter);
            File.WriteAllText(TestFilePath, jsonContent);

            Filter result = await _jsonFileProvider.ReadFilterFromFileAsync(TestFilePath);

            Assert.IsNotNull(result);
            Assert.AreEqual(filter.Title, result.Title);
            Assert.AreEqual(filter.Author, result.Author);
            Assert.AreEqual(filter.Genre, result.Genre);
            Assert.AreEqual(filter.MoreThanPages, result.MoreThanPages);
        }

        [Test]
        public void ReadFilterFromFileAsync_FileNotFound_ShouldThrowFileNotFoundException()
        {
            string nonExistentFilePath = "non_existent_file.json";

            FileNotFoundException ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await _jsonFileProvider.ReadFilterFromFileAsync(nonExistentFilePath));

            Assert.That(ex.Message, Does.Contain($"JSON file '{nonExistentFilePath}' was not found."));
        }

        [Test]
        public void ReadFilterFromFileAsync_InvalidJson_ShouldThrowException()
        {
            string invalidJsonContent = "Invalid JSON Content";
            File.WriteAllText(TestFilePath, invalidJsonContent);

            Exception ex = Assert.ThrowsAsync<Exception>(async () =>
                await _jsonFileProvider.ReadFilterFromFileAsync(TestFilePath));

            Assert.That(ex.Message, Does.StartWith($"Error parsing JSON file '{TestFilePath}':"));
        }
    }
}
