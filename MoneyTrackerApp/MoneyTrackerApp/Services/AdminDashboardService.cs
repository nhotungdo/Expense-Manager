using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using System.Globalization;

namespace MoneyTrackerApp.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync();
        Task<bool> ToggleMaintenanceModeAsync(bool enable);
    }

    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ExpenseManagerContext _context;

        public AdminDashboardService(ExpenseManagerContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetDashboardDataAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            
            // 1. KPI Cards
            var totalUsers = await _context.Users.CountAsync();
            var newUsersToday = await _context.Users.CountAsync(u => u.CreatedAt >= today);
            
            var systemAum = await _context.Accounts.SumAsync(a => a.CurrentBalance);
            
            var todayTransactions = await _context.Transactions
                .CountAsync(t => t.TransactionDate >= today && t.TransactionDate < today.AddDays(1));

            // Simulating "Error" logs by checking Action/Details text since Level column is missing
            // In a real scenario, we would add a Level column to AuditLog
            var healthWarnings = await _context.AuditLogs
                .CountAsync(l => l.CreatedAt >= today.AddDays(-1) && 
                               ((l.Action != null && (l.Action.Contains("Error") || l.Action.Contains("Failed") || l.Action.Contains("Warning"))) ||
                                (l.Details != null && (l.Details.Contains("Error") || l.Details.Contains("Exception")))));

            // 2. Charts
            
            // User Growth (Last 12 months)
            var userGrowthData = new List<UserGrowthDataDto>();
            for (int i = 11; i >= 0; i--)
            {
                var date = today.AddMonths(-i);
                var monthStart = new DateTime(date.Year, date.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                
                var count = await _context.Users.CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
                userGrowthData.Add(new UserGrowthDataDto 
                { 
                    Period = monthStart.ToString("MM/yyyy"), 
                    Count = count 
                });
            }

            // Transaction Volume (Last 7 days - simplified as Day of Week)
            var dayOfWeekData = new List<TransactionVolumeDto>();
            // We want to show trends for days of the week based on recent history (e.g. last 30 days avg or just last 7 days)
            // Let's do last 7 days for simplicity
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = await _context.Transactions
                    .CountAsync(t => t.TransactionDate >= date && t.TransactionDate < date.AddDays(1));
                
                dayOfWeekData.Add(new TransactionVolumeDto 
                { 
                    Day = date.ToString("ddd"), // Mon, Tue...
                    Count = count 
                });
            }

            // Category Allocation (Top 5 categories by usage count in system)
            var topCategories = await _context.Transactions
                .Where(t => t.CategoryId != null)
                .GroupBy(t => t.Category!.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var totalTransWithCategory = await _context.Transactions.CountAsync(t => t.CategoryId != null);
            var categoryAllocation = topCategories.Select(c => new CategoryAllocationDto
            {
                Category = c.Category ?? "Unknown",
                Percentage = totalTransWithCategory > 0 ? Math.Round((double)c.Count / totalTransWithCategory * 100, 1) : 0
            }).ToList();

            // 3. Recent Activities
            
            var newestMembers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName ?? "N/A",
                    Email = u.Email ?? "N/A",
                    CreatedAt = u.CreatedAt ?? DateTime.MinValue,
                    IsActive = u.Enabled
                })
                .ToListAsync();

            var recentLogs = await _context.AuditLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .Select(l => new AuditLogDto
                {
                    UserEmail = l.User != null ? l.User.Email ?? "Unknown" : "System",
                    Action = l.Action,
                    Time = l.CreatedAt ?? DateTime.MinValue,
                    IpAddress = l.IpAddress ?? "-"
                })
                .ToListAsync();

            // 4. System Status
            var maintenanceSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "MaintenanceMode");
            
            var isMaintenance = maintenanceSetting != null && maintenanceSetting.SettingValue == "true";

            // Database size - Mocking for now as raw SQL permissions varry
            // In SQL Server: sp_spaceused
            string dbSize = "Unknown";
            try 
            {
                // Note: This requires high privileges and might fail on some hosted envs
                // check if we can run simple query
                // Using a safe fallback
                dbSize = "Standard"; 
            }
            catch {}

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                NewUsersToday = newUsersToday,
                SystemAum = systemAum,
                TodayTransactionsCount = todayTransactions,
                SystemHealthWarnings = healthWarnings,
                UserGrowthChart = userGrowthData,
                TransactionVolumeChart = dayOfWeekData,
                CategoryAllocationChart = categoryAllocation,
                NewestMembers = newestMembers,
                RecentAuditLogs = recentLogs,
                DatabaseSize = dbSize,
                IsMaintenanceMode = isMaintenance
            };
        }

        public async Task<bool> ToggleMaintenanceModeAsync(bool enable)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "MaintenanceMode");

            if (setting == null)
            {
                setting = new SystemSetting
                {
                    SettingKey = "MaintenanceMode",
                    SettingValue = enable ? "true" : "false",
                    SettingType = "Boolean",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = enable ? "true" : "false";
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
