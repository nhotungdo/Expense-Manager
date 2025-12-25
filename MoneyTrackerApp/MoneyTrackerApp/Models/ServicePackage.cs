using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrackerApp.Models
{
    public class ServicePackage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        public int DurationDays { get; set; }

        public string Features { get; set; } = string.Empty; // JSON array of features

        public int PackageType { get; set; }
        public int BillingCycle { get; set; }
        public int MaxTransactions { get; set; }
        public int MaxAccounts { get; set; }
        public int MaxBudgets { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasAiAdvisor { get; set; }
        public bool HasGroupExpense { get; set; }
        public bool HasPrioritySupport { get; set; }

        public bool IsPopular { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string? BadgeText { get; set; }

        [StringLength(50)]
        public string? BadgeColor { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
