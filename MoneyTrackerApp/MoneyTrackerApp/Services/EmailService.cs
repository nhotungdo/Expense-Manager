using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTrackerApp.Configuration;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ExpenseManagerContext context, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _context = context;
        _logger = logger;
    }

    public async Task SendEmailAsync(List<string> toEmails, string subject, string body, List<IFormFile>? attachments = null)
    {
        foreach (var email in toEmails)
        {
            try
            {
                await SendEmailAsync(email, subject, body, attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                // Continue sending to others
            }
        }
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, List<IFormFile>? attachments = null)
    {
        var emailLog = new Email
        {
            RecipientEmail = toEmail,
            Subject = subject,
            Body = body,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Try to find user by email to link
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == toEmail);
        if (user != null)
        {
            emailLog.UserId = user.Id;
        }

        try
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var file in attachments)
                    {
                        if (file.Length > 0)
                        {
                            var stream = file.OpenReadStream();
                            var attachment = new Attachment(stream, file.FileName, file.ContentType);
                            message.Attachments.Add(attachment);
                        }
                    }
                }

                using (var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port))
                {
                    client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                    client.EnableSsl = _emailSettings.EnableSsl;
                    
                    await client.SendMailAsync(message);
                }
            }

            emailLog.Status = "Sent";
            emailLog.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            emailLog.Status = "Failed";
            _logger.LogError(ex, "Error sending email to {Recipient}", toEmail);
        }

        _context.Emails.Add(emailLog);
        await _context.SaveChangesAsync();
    }
}
