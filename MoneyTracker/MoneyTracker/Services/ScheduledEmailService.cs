using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class ScheduledEmailService : IScheduledEmailService
    {
        private readonly ExpenseManagerContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ScheduledEmailService> _logger;

        public ScheduledEmailService(
            ExpenseManagerContext context,
            IEmailService emailService,
            ILogger<ScheduledEmailService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendMonthlyReportsAsync()
        {
            try
            {
                _logger.LogInformation("Starting monthly report email job");

                var users = await _context.Users
                    .Where(u => u.Enabled && u.EmailNotifications)
                    .ToListAsync();

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                foreach (var user in users)
                {
                    try
                    {
                        var success = await _emailService.SendMonthlyReportAsync(user.Id);
                        if (success)
                        {
                            _logger.LogInformation("Monthly report sent successfully to user {UserId}", user.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to send monthly report to user {UserId}", user.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending monthly report to user {UserId}", user.Id);
                    }

                    // Add delay between emails to avoid rate limiting
                    await Task.Delay(1000);
                }

                _logger.LogInformation("Monthly report email job completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in monthly report email job");
            }
        }

        public async Task SendBudgetAlertsAsync()
        {
            try
            {
                _logger.LogInformation("Starting budget alert email job");

                var users = await _context.Users
                    .Where(u => u.Enabled && u.EmailNotifications)
                    .ToListAsync();

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                foreach (var user in users)
                {
                    try
                    {
                        // Check if user has exceeded budget
                        var monthlyIncome = await _context.Incomes
                            .Where(i => i.UserId == user.Id &&
                                       i.IncomeDate.Month == currentMonth &&
                                       i.IncomeDate.Year == currentYear)
                            .SumAsync(i => i.Amount);

                        var monthlyExpenses = await _context.Expenses
                            .Where(e => e.UserId == user.Id &&
                                       e.ExpenseDate.Month == currentMonth &&
                                       e.ExpenseDate.Year == currentYear)
                            .SumAsync(e => e.Amount);

                        if (monthlyIncome > 0)
                        {
                            var expenseRatio = (monthlyExpenses / monthlyIncome) * 100;

                            if (expenseRatio > 90)
                            {
                                var message = $"Cảnh báo: Chi tiêu tháng này đã vượt quá 90% thu nhập ({expenseRatio:F1}%). Hãy cân nhắc cắt giảm chi tiêu.";
                                await _emailService.SendBudgetAlertAsync(user.Id, message);
                                _logger.LogInformation("Budget alert sent to user {UserId}", user.Id);
                            }
                            else if (expenseRatio > 80)
                            {
                                var message = $"Cảnh báo: Chi tiêu tháng này đã vượt quá 80% thu nhập ({expenseRatio:F1}%). Hãy kiểm soát chi tiêu tốt hơn.";
                                await _emailService.SendBudgetAlertAsync(user.Id, message);
                                _logger.LogInformation("Budget alert sent to user {UserId}", user.Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending budget alert to user {UserId}", user.Id);
                    }

                    // Add delay between emails
                    await Task.Delay(500);
                }

                _logger.LogInformation("Budget alert email job completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in budget alert email job");
            }
        }

        public async Task SendWeeklySummariesAsync()
        {
            try
            {
                _logger.LogInformation("Starting weekly summary email job");

                var users = await _context.Users
                    .Where(u => u.Enabled && u.EmailNotifications)
                    .ToListAsync();

                var weekStart = DateTime.UtcNow.AddDays(-7);
                var weekEnd = DateTime.UtcNow;

                foreach (var user in users)
                {
                    try
                    {
                        var weeklyIncome = await _context.Incomes
                            .Where(i => i.UserId == user.Id &&
                                       i.IncomeDate >= DateOnly.FromDateTime(weekStart) &&
                                       i.IncomeDate <= DateOnly.FromDateTime(weekEnd))
                            .SumAsync(i => i.Amount);

                        var weeklyExpenses = await _context.Expenses
                            .Where(e => e.UserId == user.Id &&
                                       e.ExpenseDate >= DateOnly.FromDateTime(weekStart) &&
                                       e.ExpenseDate <= DateOnly.FromDateTime(weekEnd))
                            .SumAsync(e => e.Amount);

                        var savings = weeklyIncome - weeklyExpenses;

                        var htmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; margin: 20px;'>
                            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                                <h2 style='color: #007bff;'>📊 Tóm tắt tuần</h2>
                                <p>Xin chào {user.FullName ?? user.Username},</p>
                                
                                <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                    <h3 style='color: #495057; margin-top: 0;'>Tóm tắt tuần qua</h3>
                                    <table style='width: 100%; border-collapse: collapse;'>
                                        <tr>
                                            <td style='padding: 10px; border-bottom: 2px solid #dee2e6; font-weight: bold;'>Thu nhập tuần:</td>
                                            <td style='padding: 10px; border-bottom: 2px solid #dee2e6; text-align: right; color: #28a745; font-weight: bold;'>{weeklyIncome:N0} ₫</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 10px; border-bottom: 2px solid #dee2e6; font-weight: bold;'>Chi tiêu tuần:</td>
                                            <td style='padding: 10px; border-bottom: 2px solid #dee2e6; text-align: right; color: #dc3545; font-weight: bold;'>{weeklyExpenses:N0} ₫</td>
                                        </tr>
                                        <tr>
                                            <td style='padding: 10px; font-weight: bold;'>Tiết kiệm:</td>
                                            <td style='padding: 10px; text-align: right; color: {(savings >= 0 ? "#28a745" : "#dc3545")}; font-weight: bold;'>{savings:N0} ₫</td>
                                        </tr>
                                    </table>
                                </div>

                                <p>Hãy truy cập <a href='#' style='color: #007bff;'>MoneyTracker</a> để xem chi tiết và quản lý tài chính của bạn.</p>
                                
                                <hr style='margin: 20px 0; border: none; border-top: 1px solid #dee2e6;'>
                                <p style='font-size: 12px; color: #6c757d;'>
                                    Email này được gửi tự động từ hệ thống MoneyTracker.<br>
                                    Nếu bạn không muốn nhận email này, vui lòng cập nhật cài đặt trong tài khoản của bạn.
                                </p>
                            </div>
                        </body>
                        </html>";

                        var subject = $"Tóm tắt tuần - MoneyTracker ({weekStart:dd/MM} - {weekEnd:dd/MM})";
                        await _emailService.SendEmailAsync(user.Email, subject, htmlBody);
                        _logger.LogInformation("Weekly summary sent to user {UserId}", user.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending weekly summary to user {UserId}", user.Id);
                    }

                    // Add delay between emails
                    await Task.Delay(500);
                }

                _logger.LogInformation("Weekly summary email job completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in weekly summary email job");
            }
        }

        public async Task ScheduleEmailAsync(long userId, string subject, string body, DateTime scheduledTime)
        {
            try
            {
                var email = new Email
                {
                    UserId = userId,
                    Subject = subject,
                    Body = body,
                    Status = "SCHEDULED",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Emails.Add(email);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Email scheduled for user {UserId} at {ScheduledTime}", userId, scheduledTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling email for user {UserId}", userId);
                throw;
            }
        }
    }
}
