namespace LibraryNetwork.ViewModels
{
    public class BorrowVM
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public string Author { get; set; } = "";
        public string? ImageUrl { get; set; }
        public List<AvailableCopyVM> AvailableCopies { get; set; } = new();
    }

    public class AvailableCopyVM
    {
        public int BookCopyId { get; set; }
        public string InventoryCode { get; set; } = "";
        public string LibraryName { get; set; } = "";
        public float PricePerDay { get; set; }
    }
}