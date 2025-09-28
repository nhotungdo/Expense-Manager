namespace MoneyTracker.Models.DTOs
{
    public class ExpenseDto
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public DateOnly ExpenseDate { get; set; }
        public string? Note { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateExpenseDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public DateOnly ExpenseDate { get; set; }
        public string? Note { get; set; }
        public long CategoryId { get; set; }
    }

    public class UpdateExpenseDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public DateOnly ExpenseDate { get; set; }
        public string? Note { get; set; }
        public long CategoryId { get; set; }
    }

    public class ExpenseFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? CategoryId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SearchTerm { get; set; }
    }
}
