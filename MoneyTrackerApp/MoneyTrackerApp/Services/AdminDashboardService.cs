using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    }

    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ExpenseManagerContext _context;

        public AdminDashboardService(ExpenseManagerContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-7);

            // Execute sequentially to avoid DbContext concurrency issues
            var activeUsers = await _context.Users.AsNoTracking().CountAsync(u => u.Enabled, cancellationToken);
            var newUsers = await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= sevenDaysAgo, cancellationToken);
            var totalBalance = await _context.Accounts.AsNoTracking()
                .SumAsync(a => (decimal?)a.CurrentBalance ?? 0m, cancellationToken);
            var txToday = await _context.Transactions.AsNoTracking()
                .CountAsync(t => t.TransactionDate >= today && t.TransactionDate < today.AddDays(1), cancellationToken);

            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Take(8)
                .Select(l => new AdminAuditLogDto
                {
                    UserEmail = l.User != null ? (l.User.Email ?? "Unknown") : "System",
                    Action = l.Action ?? "N/A",
                    Time = l.CreatedAt ?? DateTime.MinValue,
                    IpAddress = l.IpAddress ?? "-"
                })
                .ToListAsync(cancellationToken);

            var metrics = new List<SystemMetricDto>
            {
                new()
                {
                    Name = "Database",
                    Status = "Online",
                    Detail = "Audit trail reachable",
                    Tone = "success"
                },
                new()
                {
                    Name = "Background jobs",
                    Status = "Healthy",
                    Detail = "Schedulers responding",
                    Tone = "success"
                },
                new()
                {
                    Name = "Payments",
                    Status = "Monitoring",
                    Detail = "VNPay events tracked",
                    Tone = "warning"
                }
            };

            return new AdminDashboardDto
            {
                ActiveUsers = activeUsers,
                NewUsersLast7Days = newUsers,
                TotalBalance = totalBalance,
                TransactionsToday = txToday,
                SystemMetrics = metrics,
                RecentAuditLogs = auditLogs
            };
        }
    }
}
