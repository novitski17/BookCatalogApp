using BookCatalogApp.Core.Interfaces;
using BookCatalogApp.Models.Models;
using Newtonsoft.Json;

namespace BookCatalogApp.Data.Providers
{
    public class JsonFileProvider : IJsonFileProvider
    {
        public async Task<Filter> ReadFilterFromFileAsync(string filePath)
        {
            try
            {

                string filterJson = await File.ReadAllTextAsync(filePath);
                Filter filter = JsonConverter.DeserializeObject<Filter>(filterJson);

                return filter;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"JSON file '{filePath}' was not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing JSON file '{filePath}': {ex.Message}");
            }
        }
    }
}
