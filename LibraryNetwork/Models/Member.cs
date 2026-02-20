namespace LibraryNetwork.Models
{
    public class Member
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? MembershipNumber { get; set; }

        public string? phoneNumber { get; set; }

        public string? ImageUrl { get; set; }

        // FK -> ApplicationUser (nullable until linked)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

        public string FullName => $"{FirstName} {LastName}";
    }
}
