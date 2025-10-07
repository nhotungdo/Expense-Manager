using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public interface IScheduledEmailService
    {
        Task SendMonthlyReportsAsync();
        Task SendBudgetAlertsAsync();
        Task SendWeeklySummariesAsync();
        Task ScheduleEmailAsync(long userId, string subject, string body, DateTime scheduledTime);
    }
}
