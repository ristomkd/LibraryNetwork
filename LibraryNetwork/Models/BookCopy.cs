namespace LibraryNetwork.Models
{
    public class BookCopy
    {
        public int Id { get; set; }
        public string? InventoryCode { get; set; } //barcode, QR code, etc.
        public bool IsAvailable { get; set; }

        public float? PricePerDay { get; set; }
        // FK -> Library
        public int LibraryId { get; set; }
        public Library? Library { get; set; }

        // FK -> Book
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
