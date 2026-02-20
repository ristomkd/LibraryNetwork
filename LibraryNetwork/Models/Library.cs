namespace LibraryNetwork.Models
{
    public class Library
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }

        public string? City { get; set; }

        public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
        public ICollection<Librarian> Librarians { get; set; } = new List<Librarian>();

    }
}
