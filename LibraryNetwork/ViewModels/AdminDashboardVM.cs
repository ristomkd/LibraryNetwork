namespace LibraryNetwork.ViewModels
{
    public class AdminDashboardVM
    {
        public string LibraryName { get; set; } = "";
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public List<RecentLoanVM> RecentLoans { get; set; } = new();
    }

    public class RecentLoanVM
    {
        public int LoanId { get; set; }
        public string BookTitle { get; set; } = "";
        public string MemberName { get; set; } = "";
        public string InventoryCode { get; set; } = "";
        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "";
        public bool IsOverdue { get; set; }
    }
}