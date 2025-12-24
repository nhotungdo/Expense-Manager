using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, List<IFormFile>? attachments = null);
    Task SendEmailAsync(List<string> toEmails, string subject, string body, List<IFormFile>? attachments = null);
}
