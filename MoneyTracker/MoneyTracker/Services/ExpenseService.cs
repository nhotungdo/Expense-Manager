using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<ExpenseService> _logger;
        private readonly IAuditService _auditService;

        public ExpenseService(ExpenseManagerContext context, ILogger<ExpenseService> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Expense>> GetExpensesAsync(long userId, int skip = 0, int take = 50)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .OrderByDescending(e => e.ExpenseDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Expense?> GetExpenseByIdAsync(long id, long userId)
        {
            return await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        public async Task<Expense> CreateExpenseAsync(ExpenseDto expenseDto, long userId)
        {
            var expense = new Expense
            {
                UserId = userId,
                Amount = expenseDto.Amount,
                CategoryId = expenseDto.CategoryId,
                ExpenseDate = expenseDto.ExpenseDate,
                Note = expenseDto.Note,
                Currency = expenseDto.Currency ?? "VND",
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "CREATE", $"Created expense: {expense.Amount:C} for category {expense.CategoryId}", "Expense", expense.Id);

            _logger.LogInformation("Expense created for user {UserId}: {ExpenseId}", userId, expense.Id);
            return expense;
        }

        public async Task<Expense?> UpdateExpenseAsync(long id, ExpenseDto expenseDto, long userId)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return null;

            var oldAmount = expense.Amount;
            var oldCategoryId = expense.CategoryId;

            expense.Amount = expenseDto.Amount;
            expense.CategoryId = expenseDto.CategoryId;
            expense.ExpenseDate = expenseDto.ExpenseDate;
            expense.Note = expenseDto.Note;
            expense.Currency = expenseDto.Currency ?? expense.Currency;

            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "UPDATE",
                $"Updated expense: Amount {oldAmount:C} -> {expense.Amount:C}, Category {oldCategoryId} -> {expense.CategoryId}",
                "Expense", expense.Id);

            _logger.LogInformation("Expense updated for user {UserId}: {ExpenseId}", userId, expense.Id);
            return expense;
        }

        public async Task<bool> DeleteExpenseAsync(long id, long userId)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return false;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "DELETE", $"Deleted expense: {expense.Amount:C}", "Expense", expense.Id);

            _logger.LogInformation("Expense deleted for user {UserId}: {ExpenseId}", userId, expense.Id);
            return true;
        }

        public async Task<decimal> GetTotalExpensesAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Expenses.Where(e => e.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= DateOnly.FromDateTime(startDate.Value));

            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= DateOnly.FromDateTime(endDate.Value));

            return await query.SumAsync(e => e.Amount);
        }

        public async Task<IEnumerable<Expense>> GetExpensesByCategoryAsync(long userId, long categoryId, int skip = 0, int take = 50)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId && e.CategoryId == categoryId)
                .Include(e => e.Category)
                .OrderByDescending(e => e.ExpenseDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetExpensesByCategorySummaryAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Expenses
                .Where(e => e.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= DateOnly.FromDateTime(startDate.Value));

            if (endDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= DateOnly.FromDateTime(endDate.Value));

            return await query
                .Include(e => e.Category)
                .GroupBy(e => e.Category != null ? e.Category.Name : "Uncategorized")
                .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public async Task<IEnumerable<Expense>> GetRecentExpensesAsync(long userId, int count = 10)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
