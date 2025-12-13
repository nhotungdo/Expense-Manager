using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersLast7Days { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal SystemAum { get; set; }
        public int TransactionsToday { get; set; }
        public int TodayTransactionsCount { get; set; }
        public int SystemHealthWarnings { get; set; }
        public IReadOnlyList<SystemMetricDto> SystemMetrics { get; set; } = Array.Empty<SystemMetricDto>();
        public IReadOnlyList<AdminAuditLogDto> RecentAuditLogs { get; set; } = Array.Empty<AdminAuditLogDto>();
        public IReadOnlyList<DailyTransactionVolumeDto> TransactionVolumeChart { get; set; } = Array.Empty<DailyTransactionVolumeDto>();
    }

    public class SystemMetricDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Tone { get; set; } = "info";
    }

    public class AdminAuditLogDto
    {
        public string UserEmail { get; set; } = "Unknown";
        public string Action { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string IpAddress { get; set; } = "-";
    }

    public class DailyTransactionVolumeDto
    {
        public string Day { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
