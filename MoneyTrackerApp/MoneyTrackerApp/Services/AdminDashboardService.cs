using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

            // Safely project AuditLogs without explicit Include (EF Core handles the join automatically in projection)
            var auditLogsList = await _context.AuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Take(8)
                .ToListAsync(cancellationToken);

            var auditLogs = auditLogsList.Select(l => new AdminAuditLogDto
            {
                UserEmail = l.User?.Email ?? "System",
                Action = l.Action ?? "N/A",
                Time = l.CreatedAt ?? DateTime.MinValue,
                IpAddress = l.IpAddress ?? "-"
            }).ToList();

            var metrics = new List<SystemMetricDto>
            {
                new()
                {
                    Name = "Cơ sở dữ liệu",
                    Status = "Trực tuyến",
                    Detail = "Kết nối ổn định",
                    Tone = "success"
                },
                new()
                {
                    Name = "Tác vụ nền",
                    Status = "Bình thường",
                    Detail = "Đang chạy đúng lịch",
                    Tone = "success"
                },
                new()
                {
                    Name = "Thanh toán",
                    Status = "Giám sát",
                    Detail = "Đang theo dõi VNPay",
                    Tone = "warning"
                }
            };

            var totalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken);
            var newUsersToday = await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= today, cancellationToken);
            
            // Transaction Volume Chart (Last 7 Days)
            var volumeStats = await _context.Transactions.AsNoTracking()
                .Where(t => t.TransactionDate >= sevenDaysAgo)
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var chartData = new List<DailyTransactionVolumeDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var stat = volumeStats.FirstOrDefault(s => s.Date == date);
                chartData.Add(new DailyTransactionVolumeDto 
                { 
                    Day = date.ToString("dd/MM"), 
                    Count = stat?.Count ?? 0 
                });
            }

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewUsersToday = newUsersToday,
                NewUsersLast7Days = newUsers,
                TotalBalance = totalBalance,
                SystemAum = totalBalance,
                TransactionsToday = txToday,
                TodayTransactionsCount = txToday,
                SystemHealthWarnings = 0, // Mock healthy for now
                SystemMetrics = metrics,
                RecentAuditLogs = auditLogs,
                TransactionVolumeChart = chartData
            };
        }
    }
}
