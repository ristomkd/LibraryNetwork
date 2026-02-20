namespace LibraryNetwork.ViewModels
{
    public class LibraryInventoryVM
    {
        public int LibraryId { get; set; }
        public string LibraryName { get; set; } = "";
        public string City { get; set; } = "";
        public string Address { get; set; } = "";

        public List<BookInventoryVM> Books { get; set; } = new();
    }

    public class BookInventoryVM
    {
        public int BookId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";

        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        public bool HasAvailable => AvailableCopies > 0;
    }
}
