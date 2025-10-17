using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyTracker.Models;

namespace MoneyTracker.Services;

public class EmailSenderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(IServiceProvider services, ILogger<EmailSenderService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ExpenseManagerContext>();

                var pending = await db.Emails
                    .Include(e => e.User)
                    .Where(e => e.Status == "Pending")
                    .OrderBy(e => e.CreatedAt)
                    .Take(25)
                    .ToListAsync(stoppingToken);

                foreach (var email in pending)
                {
                    try
                    {
                        // Placeholder: integrate with real SMTP/provider
                        var recipient = email.User?.Email ?? "recipient@example.com";
                        using var msg = new MailMessage("no-reply@moneytracker.local", recipient)
                        {
                            Subject = email.Subject ?? "",
                            Body = email.Body ?? "",
                            IsBodyHtml = true
                        };
                        // pretend sent
                        email.Status = "Sent";
                        email.SentAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email {EmailId}", email.Id);
                        email.Status = "Failed";
                    }
                }

                if (pending.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailSenderService loop error");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}


