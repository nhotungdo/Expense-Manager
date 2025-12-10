using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.DTOs
{
    public class AdminDashboardDto
    {
        public int ActiveUsers { get; set; }
        public int NewUsersLast7Days { get; set; }
        public decimal TotalBalance { get; set; }
        public int TransactionsToday { get; set; }
        public IReadOnlyList<SystemMetricDto> SystemMetrics { get; set; } = Array.Empty<SystemMetricDto>();
        public IReadOnlyList<AdminAuditLogDto> RecentAuditLogs { get; set; } = Array.Empty<AdminAuditLogDto>();
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
}
