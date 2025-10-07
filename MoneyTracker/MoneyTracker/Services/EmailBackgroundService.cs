using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Services;

namespace MoneyTracker.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Check every hour

        public EmailBackgroundService(IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var scheduledEmailService = scope.ServiceProvider.GetRequiredService<IScheduledEmailService>();

                    var currentTime = DateTime.UtcNow;
                    var currentHour = currentTime.Hour;
                    var currentDay = currentTime.Day;
                    var currentDayOfWeek = currentTime.DayOfWeek;

                    // Send weekly summaries every Monday at 9 AM
                    if (currentDayOfWeek == DayOfWeek.Monday && currentHour == 9)
                    {
                        _logger.LogInformation("Sending weekly summaries");
                        await scheduledEmailService.SendWeeklySummariesAsync();
                    }

                    // Send budget alerts every day at 8 PM
                    if (currentHour == 20)
                    {
                        _logger.LogInformation("Sending budget alerts");
                        await scheduledEmailService.SendBudgetAlertsAsync();
                    }

                    // Send monthly reports on the 1st of each month at 10 AM
                    if (currentDay == 1 && currentHour == 10)
                    {
                        _logger.LogInformation("Sending monthly reports");
                        await scheduledEmailService.SendMonthlyReportsAsync();
                    }

                    // Process scheduled emails every hour
                    await ProcessScheduledEmails(scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Email Background Service");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Email Background Service stopped");
        }

        private async Task ProcessScheduledEmails(IServiceProvider serviceProvider)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ExpenseManagerContext>();
                var emailService = serviceProvider.GetRequiredService<IEmailService>();

                var scheduledEmails = await context.Emails
                    .Where(e => e.Status == "SCHEDULED" && e.CreatedAt <= DateTime.UtcNow)
                    .Take(10) // Process 10 emails at a time
                    .ToListAsync();

                foreach (var email in scheduledEmails)
                {
                    try
                    {
                        var success = await emailService.SendEmailAsync(
                            email.User?.Email ?? "",
                            email.Subject,
                            email.Body);

                        email.Status = success ? "SENT" : "FAILED";
                        email.SentAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing scheduled email {EmailId}", email.Id);
                        email.Status = "FAILED";
                    }
                }

                if (scheduledEmails.Any())
                {
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Processed {Count} scheduled emails", scheduledEmails.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled emails");
            }
        }
    }
}
