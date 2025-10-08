namespace MoneyTracker.Models.DTOs
{
    public class BudgetDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PercentageUsed { get; set; }
        public string? Currency { get; set; }
        public string PeriodType { get; set; } = string.Empty; // "MONTHLY", "WEEKLY", "YEARLY"
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateBudgetDto
    {
        public long? CategoryId { get; set; }
        public decimal BudgetAmount { get; set; }
        public string? Currency { get; set; } = "VND";
        public string PeriodType { get; set; } = "MONTHLY";
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    public class UpdateBudgetDto
    {
        public long? CategoryId { get; set; }
        public decimal BudgetAmount { get; set; }
        public string? Currency { get; set; }
        public string PeriodType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class BudgetSummaryDto
    {
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal PercentageUsed { get; set; }
        public int ActiveBudgets { get; set; }
        public int OverBudgetCategories { get; set; }
        public List<BudgetDto> Budgets { get; set; } = new List<BudgetDto>();
    }
}
