using BookCatalogApp.Core.Interfaces;
using BookCatalogApp.Core.Models;
using BookCatalogApp.Models.Entities;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using BookCatalogApp.Data.Services.Validators;

namespace BookCatalogApp.Data.Providers
{
    public class CsvFileProvider : ICsvFileProvider
    {
        private readonly CsvRecordValidator _validator;

        public CsvFileProvider()
        {
            _validator = new CsvRecordValidator();
        }

        public async Task<IEnumerable<Book>> ParseCsvFileAsync(string filePath)
        {
            List<Book> books = new List<Book>();

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, CreateCsvConfiguration());

                foreach (var record in csv.GetRecords<BookCsv>())
                {
                    int rowNumber = csv.Context.Parser.Row;
                    ProcessCsvRecord(record, books, rowNumber);
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"CSV file '{filePath}' was not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing CSV file '{filePath}': {ex.Message}");
            }

            return books;
        }

        public async Task WriteBooksToCsvAsync(IEnumerable<Book> books, string outputFilePath)
        {
            try
            {
                using var writer = new StreamWriter(outputFilePath);
                using var csv = new CsvWriter(writer, CreateCsvConfiguration());

                csv.WriteHeader<BookCsv>();
                await csv.NextRecordAsync();

                foreach (var book in books)
                {
                    csv.WriteRecord(CreateCsvRecordFromBook(book));
                    await csv.NextRecordAsync();
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"CSV file '{outputFilePath}' was not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing CSV file '{outputFilePath}': {ex.Message}");
            }
        }

        private CsvConfiguration CreateCsvConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        private void ProcessCsvRecord(BookCsv record, List<Book> books, int rowNumber)
        {
            var validationErrors = _validator.Validate(record);

            if (!validationErrors.Any())
            {
                Book book = CreateBookFromRecord(record);
                books.Add(book);
            }
            else
            {
                Console.WriteLine($"Skipping record at row {rowNumber} due to validation errors: {string.Join(", ", validationErrors)}");
            }
        }

        private Book? CreateBookFromRecord(BookCsv record)
        {
            if (!DateTime.TryParse(record.ReleaseDate, out DateTime releaseDate))
            {
                throw new FormatException($"Invalid date format for book '{record.Title}'.");
            }

            if (!int.TryParse(record.Pages.ToString(), out int pages))
            {
                throw new FormatException($"Invalid page number for book '{record.Title}'");
            }

            return new Book
            {
                Title = record.Title,
                Pages = record.Pages,
                ReleaseDate = releaseDate,
                Genre = new Genre { Name = record.Genre },
                Author = new Author { Name = record.Author },
                Publisher = new Publisher { Name = record.Publisher }
            };
        }

        private BookCsv CreateCsvRecordFromBook(Book book)
        {
            return new BookCsv
            {
                Title = book.Title,
                Pages = book.Pages,
                Genre = book.Genre.Name,
                ReleaseDate = book.ReleaseDate.ToString("yyyy-MM-dd"),
                Author = book.Author.Name,
                Publisher = book.Publisher.Name
            };
        }
    }
}
