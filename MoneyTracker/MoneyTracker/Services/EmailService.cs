using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
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

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("MoneyTracker", emailSettings["FromEmail"]));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    emailSettings["SmtpHost"],
                    int.Parse(emailSettings["SmtpPort"]!),
                    bool.Parse(emailSettings["UseSsl"]!)
                );

                await client.AuthenticateAsync(
                    emailSettings["SmtpUsername"],
                    emailSettings["SmtpPassword"]
                );

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                // Save email record to database
                var emailRecord = new Email
                {
                    UserId = await GetUserIdByEmailAsync(to),
                    Subject = subject,
                    Body = body,
                    Status = "SENT",
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Emails.Add(emailRecord);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);

                // Save failed email record
                try
                {
                    var emailRecord = new Email
                    {
                        UserId = await GetUserIdByEmailAsync(to),
                        Subject = subject,
                        Body = body,
                        Status = "FAILED",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Emails.Add(emailRecord);
                    await _context.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to save failed email record");
                }

                return false;
            }
        }

        public async Task<bool> SendMonthlyReportAsync(long userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.Enabled)
                {
                    return false;
                }

                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;

                // Get monthly data
                var monthlyIncome = await _context.Incomes
                    .Where(i => i.UserId == userId &&
                               i.IncomeDate.Month == currentMonth &&
                               i.IncomeDate.Year == currentYear)
                    .SumAsync(i => i.Amount);

                var monthlyExpenses = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate.Month == currentMonth &&
                               e.ExpenseDate.Year == currentYear)
                    .SumAsync(e => e.Amount);

                var expensesByCategory = await _context.Expenses
                    .Where(e => e.UserId == userId &&
                               e.ExpenseDate.Month == currentMonth &&
                               e.ExpenseDate.Year == currentYear)
                    .Include(e => e.Category)
                    .GroupBy(e => e.Category != null ? e.Category.Name : "Uncategorized")
                    .Select(g => new { Category = g.Key, Amount = g.Sum(e => e.Amount) })
                    .ToListAsync();

                // Generate HTML report
                var htmlBody = GenerateMonthlyReportHtml(user, monthlyIncome, monthlyExpenses, expensesByCategory.Cast<object>().ToList(), currentMonth, currentYear);

                var subject = $"Báo cáo tài chính tháng {currentMonth}/{currentYear} - MoneyTracker";

                return await SendEmailAsync(user.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send monthly report to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SendBudgetAlertAsync(long userId, string message)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.Enabled)
                {
                    return false;
                }

                var htmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; margin: 20px;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #dc3545;'>⚠️ Cảnh báo ngân sách</h2>
                        <p>Xin chào {user.FullName ?? user.UserName},</p>
                        <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                            <p style='margin: 0; color: #856404;'>{message}</p>
                        </div>
                        <p>Hãy truy cập <a href='#' style='color: #007bff;'>MoneyTracker</a> để xem chi tiết và điều chỉnh chi tiêu của bạn.</p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #dee2e6;'>
                        <p style='font-size: 12px; color: #6c757d;'>
                            Email này được gửi tự động từ hệ thống MoneyTracker.<br>
                            Nếu bạn không muốn nhận email này, vui lòng cập nhật cài đặt trong tài khoản của bạn.
                        </p>
                    </div>
                </body>
                </html>";

                var subject = "Cảnh báo ngân sách - MoneyTracker";

                return await SendEmailAsync(user.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send budget alert to user {UserId}", userId);
                return false;
            }
        }

        private string GenerateMonthlyReportHtml(User user, decimal monthlyIncome, decimal monthlyExpenses,
            List<object> expensesByCategory, int month, int year)
        {
            var savings = monthlyIncome - monthlyExpenses;
            var savingsRate = monthlyIncome > 0 ? (savings / monthlyIncome) * 100 : 0;

            var categoryRows = string.Join("", expensesByCategory.Select(c =>
            {
                dynamic item = c;
                return $"<tr><td style='padding: 8px; border-bottom: 1px solid #dee2e6;'>{item.Category}</td>" +
                       $"<td style='padding: 8px; border-bottom: 1px solid #dee2e6; text-align: right;'>{item.Amount:C0}</td></tr>";
            }));

            return $@"
            <html>
            <body style='font-family: Arial, sans-serif; margin: 20px;'>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #007bff;'>📊 Báo cáo tài chính tháng {month}/{year}</h2>
                    <p>Xin chào {user.FullName ?? user.UserName},</p>
                    
                    <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #495057; margin-top: 0;'>Tổng quan tháng {month}/{year}</h3>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 10px; border-bottom: 2px solid #dee2e6; font-weight: bold;'>Tổng thu nhập:</td>
                                <td style='padding: 10px; border-bottom: 2px solid #dee2e6; text-align: right; color: #28a745; font-weight: bold;'>{monthlyIncome:C0}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border-bottom: 2px solid #dee2e6; font-weight: bold;'>Tổng chi tiêu:</td>
                                <td style='padding: 10px; border-bottom: 2px solid #dee2e6; text-align: right; color: #dc3545; font-weight: bold;'>{monthlyExpenses:C0}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border-bottom: 2px solid #dee2e6; font-weight: bold;'>Tiết kiệm:</td>
                                <td style='padding: 10px; border-bottom: 2px solid #dee2e6; text-align: right; color: {(savings >= 0 ? "#28a745" : "#dc3545")}; font-weight: bold;'>{savings:C0}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; font-weight: bold;'>Tỷ lệ tiết kiệm:</td>
                                <td style='padding: 10px; text-align: right; color: {(savingsRate >= 20 ? "#28a745" : "#ffc107")}; font-weight: bold;'>{savingsRate:F1}%</td>
                            </tr>
                        </table>
                    </div>

                    {(expensesByCategory.Count > 0 ? $@"
                    <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h3 style='color: #495057; margin-top: 0;'>Chi tiêu theo danh mục</h3>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr style='background-color: #f8f9fa;'>
                                <th style='padding: 10px; border-bottom: 2px solid #dee2e6; text-align: left;'>Danh mục</th>
                                <th style='padding: 10px; border-bottom: 2px solid #dee2e6; text-align: right;'>Số tiền</th>
                            </tr>
                            {categoryRows}
                        </table>
                    </div>" : "")}

                    <div style='background-color: {(savingsRate >= 20 ? "#d4edda" : savingsRate >= 10 ? "#fff3cd" : "#f8d7da")}; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='margin-top: 0; color: {(savingsRate >= 20 ? "#155724" : savingsRate >= 10 ? "#856404" : "#721c24")};'>
                            💡 Lời khuyên tài chính
                        </h4>
                        <p style='margin-bottom: 0; color: {(savingsRate >= 20 ? "#155724" : savingsRate >= 10 ? "#856404" : "#721c24")};'>
                            {(savingsRate >= 20 ? "Tuyệt vời! Bạn đang tiết kiệm rất tốt. Hãy duy trì thói quen này và xem xét đầu tư để tăng trưởng tài sản." :
                              savingsRate >= 10 ? "Bạn đang tiết kiệm ở mức khá tốt. Hãy cố gắng tăng tỷ lệ tiết kiệm lên 20% để có tương lai tài chính vững chắc hơn." :
                              "Tỷ lệ tiết kiệm của bạn còn thấp. Hãy xem xét cắt giảm chi tiêu không cần thiết và tăng cường tiết kiệm.")}
                        </p>
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
        }

        private async Task<long> GetUserIdByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user?.Id ?? 0;
        }
    }
}
