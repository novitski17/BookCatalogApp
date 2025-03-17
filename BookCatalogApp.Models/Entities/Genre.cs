using System.ComponentModel.DataAnnotations;

namespace BookCatalogApp.Models.Entities
{
    public class Genre
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
