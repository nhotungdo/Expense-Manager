namespace MoneyTracker.Models.DTOs
{
    public class CategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public long? UserId { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public bool IsGlobal { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsGlobal { get; set; } = false;
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Income" or "Expense"
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
