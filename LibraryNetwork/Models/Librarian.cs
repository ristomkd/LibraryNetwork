namespace LibraryNetwork.Models
{
    public class Librarian
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        public string? ImageUrl { get; set; }
        // FK -> Library
        public int LibraryId { get; set; }
        public Library? Library { get; set; }
    }
}
