namespace MoneyTracker.Models.DTOs
{
    public class CategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public long UserId { get; set; }
        public bool IsGlobal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public bool IsGlobal { get; set; } = false;
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
    }
}
