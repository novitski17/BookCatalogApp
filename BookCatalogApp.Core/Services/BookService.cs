using System.Linq.Expressions;
using BookCatalogApp.Core.Interfaces;
using BookCatalogApp.Models.Entities;
using BookCatalogApp.Models.Models;
using BookCatalogApp.Core.Extensions;
using BookCatalogApp.Core.Interfaces.Repositories;

namespace BookCatalogApp.Core.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IRepository<Genre> _genreRepository;
        private readonly IRepository<Author> _authorRepository;
        private readonly IRepository<Publisher> _publisherRepository;
        private readonly ICsvFileProvider _csvFileProvider;
        private readonly IJsonFileProvider _jsonFileProvider;

        private const string FilterFileName = "filter.json";
        private const string OutputFolder = "BookSearchHistory";

        public BookService(IBookRepository bookRepository,
                           IRepository<Genre> genreRepository,
                           IRepository<Author> authorRepository,
                           IRepository<Publisher> publisherRepository,
                           ICsvFileProvider csvFileProvider,
                           IJsonFileProvider jsonFileProvider)
        {
            _bookRepository = bookRepository;
            _genreRepository = genreRepository;
            _authorRepository = authorRepository;
            _publisherRepository = publisherRepository;
            _csvFileProvider = csvFileProvider;
            _jsonFileProvider = jsonFileProvider;
        }

        public async Task AddBooksFromFileAsync(string filePath)
        {
            try
            {
                IEnumerable<Book> books = await _csvFileProvider.ParseCsvFileAsync(filePath);
                await AddBooksAsync(books);

                Console.WriteLine("Books have been added to the database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task SearchBooksAsync(string outputDirectory)
        {
            try
            {
                Filter filter = await _jsonFileProvider.ReadFilterFromFileAsync(GetFilterFilePath());

                Expression<Func<Book, bool>> filterExpression = BuildFilterExpression(filter);
                IEnumerable<Book> books = await _bookRepository.GetBooksWithIncludesAsync(filterExpression);

                if (!books.Any())
                {
                    Console.WriteLine("No books were found based on the filter criteria.");
                    return;
                }

                Console.WriteLine($"Number of books found: {books.Count()}");
                foreach (Book book in books)
                {
                    Console.WriteLine(book.Title);
                }

                string outputFileName = GetFullOutputFilePath(outputDirectory);

                await _csvFileProvider.WriteBooksToCsvAsync(books, outputFileName);

                Console.WriteLine($"Books have been saved to {outputFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while searching books: {ex.Message}");
            }
        }

        private Expression<Func<Book, bool>> BuildFilterExpression(Filter filter)
        {
            Expression<Func<Book, bool>> expression = b => true;

            if (!string.IsNullOrEmpty(filter.Title))
            {
                expression = expression.AndAlso(b => b.Title.Contains(filter.Title));
            }

            if (!string.IsNullOrEmpty(filter.Author))
            {
                expression = expression.AndAlso(b => b.Author.Name.Contains(filter.Author));
            }

            if (!string.IsNullOrEmpty(filter.Genre))
            {
                expression = expression.AndAlso(b => b.Genre.Name.Contains(filter.Genre));
            }

            if (!string.IsNullOrEmpty(filter.Publisher))
            {
                expression = expression.AndAlso(b => b.Publisher.Name.Contains(filter.Publisher));
            }

            if (filter.MoreThanPages.HasValue)
            {
                expression = expression.AndAlso(b => b.Pages > filter.MoreThanPages.Value);
            }

            if (filter.LessThanPages.HasValue)
            {
                expression = expression.AndAlso(b => b.Pages < filter.LessThanPages.Value);
            }

            if (filter.PublishedAfter.HasValue)
            {
                expression = expression.AndAlso(b => b.ReleaseDate > filter.PublishedAfter.Value);
            }

            if (filter.PublishedBefore.HasValue)
            {
                expression = expression.AndAlso(b => b.ReleaseDate < filter.PublishedBefore.Value);
            }

            return expression;
        }

        private string GetFilterFilePath()
        {
            return Path.Combine(AppContext.BaseDirectory, FilterFileName);
        }

        private string GetFullOutputFilePath(string outputDirectory)
        {
            string fullOutputFolderPath = Path.Combine(outputDirectory, OutputFolder);

            EnsureDirectoryExists(fullOutputFolderPath);

            return GenerateFilePath(fullOutputFolderPath);
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Directory '{directoryPath}' has been created.");
            }
        }

        private string GenerateFilePath(string directoryPath)
        {
           return Path.Combine(directoryPath, $"books_output_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        private async Task AddBooksAsync(IEnumerable<Book> books)
        {
            List<string> genreNames = books.Select(b => b.Genre.Name).Distinct().ToList();
            List<string> authorNames = books.Select(b => b.Author.Name).Distinct().ToList();
            List<string> publisherNames = books.Select(b => b.Publisher.Name).Distinct().ToList();

            Dictionary<string, Genre> existingGenres = await _genreRepository.GetByNamesAsync(genreNames);
            Dictionary<string, Author> existingAuthors = await _authorRepository.GetByNamesAsync(authorNames);
            Dictionary<string, Publisher> existingPublishers = await _publisherRepository.GetByNamesAsync(publisherNames);


            List<Book> uniqueBooks = new List<Book>();

            foreach (var book in books)
            {
                if (!existingGenres.TryGetValue(book.Genre.Name, out var genre))
                {
                    await _genreRepository.AddAsync(book.Genre);
                    existingGenres[book.Genre.Name] = book.Genre;
                }
                else
                {
                    book.Genre = genre;
                }

                if (!existingAuthors.TryGetValue(book.Author.Name, out var author))
                {
                    await _authorRepository.AddAsync(book.Author);
                    existingAuthors[book.Author.Name] = book.Author;
                }
                else
                {
                    book.Author = author;
                }

                if (!existingPublishers.TryGetValue(book.Publisher.Name, out var publisher))
                {
                    await _publisherRepository.AddAsync(book.Publisher);
                    existingPublishers[book.Publisher.Name] = book.Publisher;
                }
                else
                {
                    book.Publisher = publisher;
                }

                bool existsInDatabase = await _bookRepository.ExistsAsync(b =>
                    b.Title == book.Title &&
                    b.AuthorId == book.Author.Id &&
                    b.PublisherId == book.Publisher.Id &&
                    b.ReleaseDate == book.ReleaseDate);

                bool existsInLocalList = uniqueBooks.Any(b =>
                    b.Title == book.Title &&
                    b.Author.Id == book.Author.Id &&
                    b.Publisher.Id == book.Publisher.Id &&
                    b.ReleaseDate == book.ReleaseDate);

                if (!existsInDatabase && !existsInLocalList)
                {
                    uniqueBooks.Add(book);
                }
            }

            if (uniqueBooks.Any())
            {
                await _bookRepository.AddRangeAsync(uniqueBooks);
                await _bookRepository.SaveChangesAsync();
            }
        }
    }
}
