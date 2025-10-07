using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<IncomeService> _logger;
        private readonly IAuditService _auditService;

        public IncomeService(ExpenseManagerContext context, ILogger<IncomeService> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Income>> GetIncomesAsync(long userId, int skip = 0, int take = 50)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId)
                .Include(i => i.Category)
                .OrderByDescending(i => i.IncomeDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Income?> GetIncomeByIdAsync(long id, long userId)
        {
            return await _context.Incomes
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
        }

        public async Task<Income> CreateIncomeAsync(IncomeDto incomeDto, long userId)
        {
            var income = new Income
            {
                UserId = userId,
                Amount = incomeDto.Amount,
                CategoryId = incomeDto.CategoryId,
                IncomeDate = incomeDto.IncomeDate,
                Note = incomeDto.Note,
                Currency = incomeDto.Currency ?? "VND",
                CreatedAt = DateTime.UtcNow
            };

            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "CREATE", $"Created income: {income.Amount:C} for category {income.CategoryId}", "Income", income.Id);

            _logger.LogInformation("Income created for user {UserId}: {IncomeId}", userId, income.Id);
            return income;
        }

        public async Task<Income?> UpdateIncomeAsync(long id, IncomeDto incomeDto, long userId)
        {
            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
                return null;

            var oldAmount = income.Amount;
            var oldCategoryId = income.CategoryId;

            income.Amount = incomeDto.Amount;
            income.CategoryId = incomeDto.CategoryId;
            income.IncomeDate = incomeDto.IncomeDate;
            income.Note = incomeDto.Note;
            income.Currency = incomeDto.Currency ?? income.Currency;

            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "UPDATE",
                $"Updated income: Amount {oldAmount:C} -> {income.Amount:C}, Category {oldCategoryId} -> {income.CategoryId}",
                "Income", income.Id);

            _logger.LogInformation("Income updated for user {UserId}: {IncomeId}", userId, income.Id);
            return income;
        }

        public async Task<bool> DeleteIncomeAsync(long id, long userId)
        {
            var income = await _context.Incomes
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (income == null)
                return false;

            _context.Incomes.Remove(income);
            await _context.SaveChangesAsync();

            await _auditService.LogUserActionAsync(userId, "DELETE", $"Deleted income: {income.Amount:C}", "Income", income.Id);

            _logger.LogInformation("Income deleted for user {UserId}: {IncomeId}", userId, income.Id);
            return true;
        }

        public async Task<decimal> GetTotalIncomeAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Incomes.Where(i => i.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(i => i.IncomeDate >= DateOnly.FromDateTime(startDate.Value));

            if (endDate.HasValue)
                query = query.Where(i => i.IncomeDate <= DateOnly.FromDateTime(endDate.Value));

            return await query.SumAsync(i => i.Amount);
        }

        public async Task<IEnumerable<Income>> GetIncomesByCategoryAsync(long userId, long categoryId, int skip = 0, int take = 50)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId && i.CategoryId == categoryId)
                .Include(i => i.Category)
                .OrderByDescending(i => i.IncomeDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetIncomeByCategorySummaryAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Incomes
                .Where(i => i.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(i => i.IncomeDate >= DateOnly.FromDateTime(startDate.Value));

            if (endDate.HasValue)
                query = query.Where(i => i.IncomeDate <= DateOnly.FromDateTime(endDate.Value));

            return await query
                .Include(i => i.Category)
                .GroupBy(i => i.Category != null ? i.Category.Name : "Uncategorized")
                .ToDictionaryAsync(g => g.Key, g => g.Sum(i => i.Amount));
        }

        public async Task<IEnumerable<Income>> GetRecentIncomesAsync(long userId, int count = 10)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId)
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
