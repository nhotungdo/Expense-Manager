namespace MoneyTracker.Models.DTOs
{
    public class TransactionDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Type { get; set; } = string.Empty; // "expense" or "income"
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Note { get; set; }
        public DateOnly TransactionDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTransactionDto
    {
        public long? CategoryId { get; set; }
        public string Type { get; set; } = string.Empty; // "expense" or "income"
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "VND";
        public string? Note { get; set; }
        public DateOnly TransactionDate { get; set; }
    }

    public class UpdateTransactionDto
    {
        public long? CategoryId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Note { get; set; }
        public DateOnly TransactionDate { get; set; }
    }

    public class TransactionFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? CategoryId { get; set; }
        public string? Type { get; set; } // "expense" or "income"
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } // "date", "amount", "category"
        public string? SortOrder { get; set; } // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TransactionSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetAmount { get; set; }
        public int TotalTransactions { get; set; }
        public int IncomeTransactions { get; set; }
        public int ExpenseTransactions { get; set; }
        public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
    }
}
