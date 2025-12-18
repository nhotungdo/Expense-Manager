using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, long userId);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly ExpenseManagerContext _context;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, ExpenseManagerContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, long userId)
        {
            var emailLog = new Email
            {
                UserId = userId,
                Subject = subject,
                Body = body,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                // Mock sending if credentials are placeholder
                if (username == "your-email@gmail.com")
                {
                    _logger.LogInformation($"[Mock Email] To: {toEmail}, Subject: {subject}, Body: {body}");
                    emailLog.Status = "Sent (Mock)";
                    emailLog.SentAt = DateTime.UtcNow;
                }
                else
                {
                     using var client = new SmtpClient(host, port)
                     {
                         Credentials = new NetworkCredential(username, password),
                         EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                     };

                     var mailMessage = new MailMessage
                     {
                         From = new MailAddress(fromEmail, fromName),
                         Subject = subject,
                         Body = body,
                         IsBodyHtml = true
                     };
                     mailMessage.To.Add(toEmail);

                     await client.SendMailAsync(mailMessage);
                     
                     emailLog.Status = "Sent";
                     emailLog.SentAt = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                emailLog.Status = "Failed";
                emailLog.Body += $"\n\nError: {ex.Message}";
            }
            finally
            {
                try 
                {
                    _context.Emails.Add(emailLog);
                    await _context.SaveChangesAsync();
                }
                catch(Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to log email to database");
                }
            }
        }
    }
}
