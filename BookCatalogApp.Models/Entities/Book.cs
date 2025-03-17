namespace BookCatalogApp.Models.Entities
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Pages { get; set; }
        public Guid GenreId { get; set; }
        public Genre Genre { get; set; }
        public Guid AuthorId { get; set; }
        public Author Author { get; set; }
        public Guid PublisherId { get; set; }
        public Publisher Publisher { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
