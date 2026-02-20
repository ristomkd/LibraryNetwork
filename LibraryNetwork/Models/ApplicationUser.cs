using Microsoft.AspNetCore.Identity;

namespace LibraryNetwork.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }

        public int? LibraryId { get; set; }
        public Library? Library { get; set; }

        public int? MemberId { get; set; }
    }
}
