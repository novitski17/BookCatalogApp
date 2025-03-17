using BookCatalogApp.Core.Models;

namespace BookCatalogApp.Data.Services.Validators
{
    public class CsvRecordValidator
    {
        public List<string> Validate(BookCsv record)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(record.Title))
                errors.Add("Title is required.");

            if (string.IsNullOrWhiteSpace(record.Author))
                errors.Add("Author is required.");

            if (string.IsNullOrWhiteSpace(record.Genre))
                errors.Add("Genre is required.");

            if (string.IsNullOrWhiteSpace(record.Publisher))
                errors.Add("Publisher is required.");

            if (record.Pages <= 0)
                errors.Add("Pages must be a positive integer.");

            if (!DateTime.TryParse(record.ReleaseDate, out _))
                errors.Add($"Invalid date format for ReleaseDate: {record.ReleaseDate}");

            return errors;
        }
    }
}
