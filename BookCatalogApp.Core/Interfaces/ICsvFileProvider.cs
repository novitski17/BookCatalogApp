using BookCatalogApp.Models.Entities;

namespace BookCatalogApp.Core.Interfaces
{
    public interface ICsvFileProvider
    {
        Task<IEnumerable<Book>> ParseCsvFileAsync(string filePath);
        Task WriteBooksToCsvAsync(IEnumerable<Book> books, string outputFilePath);
    }
}
