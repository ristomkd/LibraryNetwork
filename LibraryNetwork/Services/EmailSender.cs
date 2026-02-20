using Microsoft.AspNetCore.Identity.UI.Services;

namespace LibraryNetwork.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Dummy implementation - no real email sending
            return Task.CompletedTask;
        }
    }
}
