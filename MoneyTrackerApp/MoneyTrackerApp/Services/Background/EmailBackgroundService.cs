using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services.Background;

public class EmailBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public EmailBackgroundService(IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing scheduled emails.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Email Background Service stopping.");
    }

    private async Task ProcessScheduledEmailsAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ExpenseManagerContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = DateTime.UtcNow;

            var pendingEmails = await context.Emails
                .Where(e => e.Status == "Scheduled" && e.ScheduledAt <= now)
                .OrderBy(e => e.ScheduledAt)
                .Take(20) // Batch size
                .ToListAsync(stoppingToken);

            if (pendingEmails.Any())
            {
                _logger.LogInformation("Found {Count} scheduled emails to send.", pendingEmails.Count);

                foreach (var email in pendingEmails)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    try
                    {
                        // Update status to processing to avoid double sending if process takes long
                        // Though typical for single worker, keeping it simple:
                        // We will try to send. If successful, mark sent. If failed, mark failed.
                        // Since we are using an existing EmailService which creates NEW Email logs
                        // we should NOT call _emailService.SendEmailAsync directly because 
                        // that method ADDS a new log entry!
                        
                        // Let's check EmailService again.
                        // It adds a new log entry. That's a problem.
                        // We need a method in EmailService to send WITHOUT logging, or we handle the sending logic here.
                        // Code duplication is bad.
                        // Best approach: add a method to IEmailService "SendRawEmailAsync" or simply use the SMTP logic here.
                        // Or better: update the existing record directly.
                        
                        // Let's refactor proper sending logic here to avoid cluttering IEmailService 
                        // or modify EmailService to accept an existing ID? No.
                        // We will replicate the SMTP sending logic here using the same configuration.
                        // This allows us to update the EXISTING record instead of creating a duplicate one.
                        
                        await SendEmailInternalAsync(scope.ServiceProvider, email);
                        
                        email.Status = "Sent";
                        email.SentAt = DateTime.UtcNow;
                        _logger.LogInformation("Successfully sent scheduled email to {Recipient}", email.RecipientEmail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send scheduled email to {Recipient}", email.RecipientEmail);
                        email.Status = "Failed";
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }

    private async Task SendEmailInternalAsync(IServiceProvider serviceProvider, Email email)
    {
        // Get SMTP settings
        var emailSettingsOption = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<MoneyTrackerApp.Configuration.EmailSettings>>();
        var settings = emailSettingsOption.Value;

        using (var message = new System.Net.Mail.MailMessage())
        {
            message.From = new System.Net.Mail.MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(new System.Net.Mail.MailAddress(email.RecipientEmail));
            message.Subject = email.Subject;
            message.Body = email.Body;
            message.IsBodyHtml = true;

            using (var client = new System.Net.Mail.SmtpClient(settings.Host, settings.Port))
            {
                client.Credentials = new System.Net.NetworkCredential(settings.Username, settings.Password);
                client.EnableSsl = settings.EnableSsl;
                
                await client.SendMailAsync(message);
            }
        }
    }
}
