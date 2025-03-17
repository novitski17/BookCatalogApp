namespace BookCatalogApp.Core.Interfaces
{
    public interface IBookService
    {
        Task AddBooksFromFileAsync(string filePath);
        Task SearchBooksAsync(string outputDirectory);
    }
}
