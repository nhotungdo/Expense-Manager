namespace MoneyTracker.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendMonthlyReportAsync(long userId);
        Task<bool> SendBudgetAlertAsync(long userId, string message);
    }
}
