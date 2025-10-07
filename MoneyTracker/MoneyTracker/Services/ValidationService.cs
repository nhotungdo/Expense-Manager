using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;
using System.Text.RegularExpressions;

namespace MoneyTracker.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ExpenseManagerContext context, ILogger<ValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateExpenseAsync(ExpenseDto expenseDto, long userId)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate amount
            if (!IsValidAmount(expenseDto.Amount))
            {
                result.AddError("Số tiền phải lớn hơn 0 và nhỏ hơn 1 tỷ VND");
            }

            // Validate category exists and belongs to user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == expenseDto.CategoryId && c.UserId == userId);

            if (category == null)
            {
                result.AddError("Danh mục không tồn tại hoặc không thuộc về bạn");
            }
            else if (category.Type != "EXPENSE")
            {
                result.AddError("Danh mục phải là loại chi tiêu");
            }

            // Validate date
            if (expenseDto.ExpenseDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            {
                result.AddError("Ngày chi tiêu không được trong tương lai");
            }

            if (expenseDto.ExpenseDate < DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)))
            {
                result.AddError("Ngày chi tiêu không được quá 10 năm trước");
            }

            // Validate note length
            if (!string.IsNullOrEmpty(expenseDto.Note) && expenseDto.Note.Length > 255)
            {
                result.AddError("Ghi chú không được vượt quá 255 ký tự");
            }

            // Check for unusual spending patterns
            if (expenseDto.Amount > 10000000) // 10 million VND
            {
                result.AddWarning("Số tiền chi tiêu này khá lớn. Bạn có chắc chắn không?");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateIncomeAsync(IncomeDto incomeDto, long userId)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate amount
            if (!IsValidAmount(incomeDto.Amount))
            {
                result.AddError("Số tiền phải lớn hơn 0 và nhỏ hơn 1 tỷ VND");
            }

            // Validate category exists and belongs to user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == incomeDto.CategoryId && c.UserId == userId);

            if (category == null)
            {
                result.AddError("Danh mục không tồn tại hoặc không thuộc về bạn");
            }
            else if (category.Type != "INCOME")
            {
                result.AddError("Danh mục phải là loại thu nhập");
            }

            // Validate date
            if (incomeDto.IncomeDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            {
                result.AddError("Ngày thu nhập không được trong tương lai");
            }

            if (incomeDto.IncomeDate < DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)))
            {
                result.AddError("Ngày thu nhập không được quá 10 năm trước");
            }

            // Validate note length
            if (!string.IsNullOrEmpty(incomeDto.Note) && incomeDto.Note.Length > 255)
            {
                result.AddError("Ghi chú không được vượt quá 255 ký tự");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateCategoryAsync(CategoryDto categoryDto, long userId)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate name
            if (string.IsNullOrWhiteSpace(categoryDto.Name))
            {
                result.AddError("Tên danh mục không được để trống");
            }
            else if (categoryDto.Name.Length > 100)
            {
                result.AddError("Tên danh mục không được vượt quá 100 ký tự");
            }

            // Validate type
            if (string.IsNullOrWhiteSpace(categoryDto.Type))
            {
                result.AddError("Loại danh mục không được để trống");
            }
            else if (categoryDto.Type != "EXPENSE" && categoryDto.Type != "INCOME")
            {
                result.AddError("Loại danh mục phải là EXPENSE hoặc INCOME");
            }

            // Validate description length
            if (!string.IsNullOrEmpty(categoryDto.Description) && categoryDto.Description.Length > 500)
            {
                result.AddError("Mô tả không được vượt quá 500 ký tự");
            }

            // Check for duplicate category name
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId &&
                                        c.Name.ToLower() == categoryDto.Name.ToLower() &&
                                        c.Type == categoryDto.Type);

            if (existingCategory != null)
            {
                result.AddError("Đã tồn tại danh mục với tên này");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateUserAsync(UserDto userDto)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate email
            if (string.IsNullOrWhiteSpace(userDto.Email))
            {
                result.AddError("Email không được để trống");
            }
            else if (!IsValidEmail(userDto.Email))
            {
                result.AddError("Email không hợp lệ");
            }
            else
            {
                // Check for duplicate email
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == userDto.Email.ToLower());

                if (existingUser != null)
                {
                    result.AddError("Email đã được sử dụng");
                }
            }

            // Validate username
            if (string.IsNullOrWhiteSpace(userDto.Username))
            {
                result.AddError("Tên người dùng không được để trống");
            }
            else if (userDto.Username.Length < 3)
            {
                result.AddError("Tên người dùng phải có ít nhất 3 ký tự");
            }
            else if (userDto.Username.Length > 50)
            {
                result.AddError("Tên người dùng không được vượt quá 50 ký tự");
            }
            else if (!Regex.IsMatch(userDto.Username, @"^[a-zA-Z0-9_]+$"))
            {
                result.AddError("Tên người dùng chỉ được chứa chữ cái, số và dấu gạch dưới");
            }

            // Validate full name
            if (!string.IsNullOrEmpty(userDto.FullName) && userDto.FullName.Length > 100)
            {
                result.AddError("Họ tên không được vượt quá 100 ký tự");
            }

            // Validate phone number
            if (!string.IsNullOrEmpty(userDto.PhoneNumber))
            {
                if (!Regex.IsMatch(userDto.PhoneNumber, @"^[0-9+\-\s()]+$"))
                {
                    result.AddError("Số điện thoại không hợp lệ");
                }
            }

            return result;
        }

        public async Task<ValidationResult> ValidateBudgetAsync(decimal amount, long userId, long categoryId)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate amount
            if (!IsValidAmount(amount))
            {
                result.AddError("Số tiền ngân sách phải lớn hơn 0 và nhỏ hơn 1 tỷ VND");
            }

            // Validate category exists and belongs to user
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

            if (category == null)
            {
                result.AddError("Danh mục không tồn tại hoặc không thuộc về bạn");
            }
            else if (category.Type != "EXPENSE")
            {
                result.AddError("Ngân sách chỉ có thể được đặt cho danh mục chi tiêu");
            }

            // Check for existing budget
            var existingBudget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId &&
                                        b.CategoryId == categoryId &&
                                        b.IsActive);

            if (existingBudget != null)
            {
                result.AddWarning("Đã tồn tại ngân sách cho danh mục này. Ngân sách cũ sẽ bị vô hiệu hóa.");
            }

            return result;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidAmount(decimal amount)
        {
            return amount > 0 && amount <= 1000000000; // 1 billion VND
        }

        public bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return startDate <= endDate &&
                   startDate >= DateTime.UtcNow.AddYears(-10) &&
                   endDate <= DateTime.UtcNow.AddYears(1);
        }
    }
}
