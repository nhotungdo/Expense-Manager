using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyTracker.Models;

namespace MoneyTracker.Services
{
    public class DailyReminderService : BackgroundService
    {
        private readonly ILogger<DailyReminderService> _logger;
        private readonly IServiceProvider _provider;

        public DailyReminderService(ILogger<DailyReminderService> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nowUtc = DateTime.UtcNow;
                var nextRun = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 5, 0, DateTimeKind.Utc);
                if (nowUtc > nextRun) nextRun = nextRun.AddDays(1);
                var delay = nextRun - nowUtc;
                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _provider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ExpenseManagerContext>();
                    var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var yesterdayDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1));
                    var tomorrowDate = yesterdayDate.AddDays(1);

                    var users = await db.Users.Where(u => u.Enabled).ToListAsync(stoppingToken);
                    foreach (var user in users)
                    {
                        var totalYesterday = await db.Expenses
                            .Where(e => e.UserId == user.Id && e.ExpenseDate >= yesterdayDate && e.ExpenseDate < tomorrowDate)
                            .SumAsync(e => (decimal?)e.Amount, stoppingToken) ?? 0m;

                        var html = new StringBuilder()
                            .Append("<h3>Money Tracker</h3>")
                            .Append($"<p>Xin chào {user.FullName ?? user.Email},</p>")
                            .Append($"<p>Tổng chi tiêu hôm qua: <b>{totalYesterday:N0}</b></p>")
                            .Append("<p><a href=\"/Dashboard\" style=\"padding:8px 12px;background:#0ea5a4;color:#fff;text-decoration:none;border-radius:6px\">Xem chi tiết</a></p>")
                            .ToString();

                        if (!string.IsNullOrWhiteSpace(user.Email))
                        {
                            await email.SendAsync(user.Email, "Nhắc nhở chi tiêu hằng ngày", html);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Daily reminder failed");
                }
            }
        }
    }
}

