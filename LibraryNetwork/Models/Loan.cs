using System.ComponentModel.DataAnnotations;
using static LibraryNetwork.Models.Enums;

namespace LibraryNetwork.Models
{
    public class Loan
    {
        public int Id { get; set; }
        // FK -> Member
        public int MemberId { get; set; }
        public Member? Member { get; set; }
        // FK -> BookCopy
        public int BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }

        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public float? FineAmount { get; set; }   // колку треба да плати
        public bool IsFinePaid { get; set; } = false;
        public DateTime? FinePaidDate { get; set; }

        public bool IsActive => ReturnDate == null && (Status == LoanStatus.Active || Status == LoanStatus.Overdue);
        public bool IsOverdue => ReturnDate == null && DueDate.HasValue && DateTime.UtcNow.Date > DueDate.Value.Date;


        public SemesterType Semester { get; set; } = SemesterType.Winter;
        public LoanStatus Status { get; set; } = LoanStatus.Reserved;
    }
}
