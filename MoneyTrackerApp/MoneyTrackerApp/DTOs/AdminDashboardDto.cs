using System;
using System.Collections.Generic;

namespace MoneyTrackerApp.DTOs
{
    public class AdminDashboardDto
    {
        // KPI Cards
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public decimal SystemAum { get; set; }
        public int TodayTransactionsCount { get; set; }
        public int SystemHealthWarnings { get; set; }

        // Charts
        public List<UserGrowthDataDto> UserGrowthChart { get; set; } = new();
        public List<TransactionVolumeDto> TransactionVolumeChart { get; set; } = new();
        public List<CategoryAllocationDto> CategoryAllocationChart { get; set; } = new();

        // Recent Activities
        public List<AdminUserDto> NewestMembers { get; set; } = new();
        public List<AuditLogDto> RecentAuditLogs { get; set; } = new();

        // System Status
        public string DatabaseSize { get; set; } = "Unknown";
        public bool IsMaintenanceMode { get; set; }
    }

    public class UserGrowthDataDto 
    { 
        public string Period { get; set; } = ""; 
        public int Count { get; set; } 
    }

    public class TransactionVolumeDto 
    { 
        public string Day { get; set; } = ""; 
        public int Count { get; set; } 
    }

    public class CategoryAllocationDto 
    { 
        public string Category { get; set; } = ""; 
        public double Percentage { get; set; } 
    }

    public class AuditLogDto 
    { 
        public string UserEmail { get; set; } = "Unknown"; 
        public string Action { get; set; } = ""; 
        public DateTime Time { get; set; } 
        public string IpAddress { get; set; } = "";
    }
}
