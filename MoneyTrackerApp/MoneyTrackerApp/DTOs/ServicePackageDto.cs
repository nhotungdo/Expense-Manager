namespace MoneyTrackerApp.DTOs
{
    public class ServicePackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int DurationDays { get; set; }
        public string BillingCycleName { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public bool IsPopular { get; set; }
        public string? BadgeText { get; set; }
        public string? BadgeColor { get; set; }
        public int DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int MaxTransactions { get; set; }
        public int MaxAccounts { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasAiAdvisor { get; set; }
        public bool HasGroupExpense { get; set; }
        public bool HasPrioritySupport { get; set; }
    }

    public class CreateServicePackageDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int DurationDays { get; set; }
        public List<string> Features { get; set; } = new();
        public bool IsPopular { get; set; }
        public string? BadgeText { get; set; }
        public string? BadgeColor { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateServicePackageDto : CreateServicePackageDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}
