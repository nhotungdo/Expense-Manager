using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public interface IValidationService
    {
        Task<ValidationResult> ValidateExpenseAsync(ExpenseDto expenseDto, long userId);
        Task<ValidationResult> ValidateIncomeAsync(IncomeDto incomeDto, long userId);
        Task<ValidationResult> ValidateCategoryAsync(CategoryDto categoryDto, long userId);
        Task<ValidationResult> ValidateUserAsync(UserDto userDto);
        Task<ValidationResult> ValidateBudgetAsync(decimal amount, long userId, long categoryId);
        bool IsValidEmail(string email);
        bool IsValidAmount(decimal amount);
        bool IsValidDateRange(DateTime startDate, DateTime endDate);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
    }
}
