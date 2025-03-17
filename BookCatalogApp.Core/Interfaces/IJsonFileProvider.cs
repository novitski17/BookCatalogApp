using BookCatalogApp.Models.Models;

namespace BookCatalogApp.Core.Interfaces
{
    public interface IJsonFileProvider
    {
        Task<Filter> ReadFilterFromFileAsync(string filePath);
    }
}
