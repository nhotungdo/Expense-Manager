using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;

namespace MoneyTracker.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(ExpenseManagerContext context, ILogger<TransactionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(long userId, TransactionFilterDto filter)
        {
            try
            {
                var query = _context.Transactions
                    .Where(t => t.UserId == userId)
                    .Include(t => t.Category)
                    .AsQueryable();

                // Apply filters
                if (filter.StartDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= DateOnly.FromDateTime(filter.StartDate.Value));
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate <= DateOnly.FromDateTime(filter.EndDate.Value));
                }

                if (filter.CategoryId.HasValue)
                {
                    query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
                }

                if (!string.IsNullOrEmpty(filter.Type))
                {
                    query = query.Where(t => t.Type.ToLower() == filter.Type.ToLower());
                }

                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(t => t.Amount >= filter.MinAmount.Value);
                }

                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(t => t.Amount <= filter.MaxAmount.Value);
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(t => t.Note!.Contains(filter.SearchTerm));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "amount" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(t => t.Amount)
                        : query.OrderByDescending(t => t.Amount),
                    "category" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(t => t.Category!.Name)
                        : query.OrderByDescending(t => t.Category!.Name),
                    _ => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(t => t.TransactionDate)
                        : query.OrderByDescending(t => t.TransactionDate)
                };

                var transactions = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category != null ? t.Category.Name : "Khác",
                        Type = t.Type,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Note = t.Note,
                        TransactionDate = t.TransactionDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TransactionDto?> GetTransactionByIdAsync(long id, long userId)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Where(t => t.Id == id && t.UserId == userId)
                    .Include(t => t.Category)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category != null ? t.Category.Name : "Khác",
                        Type = t.Type,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Note = t.Note,
                        TransactionDate = t.TransactionDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction {TransactionId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<TransactionDto> CreateTransactionAsync(long userId, CreateTransactionDto createDto)
        {
            try
            {
                var transaction = new Transaction
                {
                    UserId = userId,
                    CategoryId = createDto.CategoryId,
                    Type = createDto.Type.ToLower(),
                    Amount = createDto.Amount,
                    Currency = createDto.Currency ?? "VND",
                    Note = createDto.Note,
                    TransactionDate = createDto.TransactionDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Also create in the appropriate table (Expense or Income)
                if (createDto.Type.ToLower() == "expense")
                {
                    var expense = new Expense
                    {
                        UserId = userId,
                        CategoryId = createDto.CategoryId,
                        Amount = createDto.Amount,
                        Currency = createDto.Currency ?? "VND",
                        Note = createDto.Note,
                        ExpenseDate = createDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Expenses.Add(expense);
                }
                else if (createDto.Type.ToLower() == "income")
                {
                    var income = new Income
                    {
                        UserId = userId,
                        CategoryId = createDto.CategoryId,
                        Amount = createDto.Amount,
                        Currency = createDto.Currency ?? "VND",
                        Note = createDto.Note,
                        IncomeDate = createDto.TransactionDate,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Incomes.Add(income);
                }

                await _context.SaveChangesAsync();

                // Return the created transaction
                return await GetTransactionByIdAsync(transaction.Id, userId) ??
                    throw new InvalidOperationException("Failed to retrieve created transaction");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TransactionDto?> UpdateTransactionAsync(long id, long userId, UpdateTransactionDto updateDto)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null)
                {
                    return null;
                }

                transaction.CategoryId = updateDto.CategoryId;
                transaction.Type = updateDto.Type.ToLower();
                transaction.Amount = updateDto.Amount;
                transaction.Currency = updateDto.Currency ?? "VND";
                transaction.Note = updateDto.Note;
                transaction.TransactionDate = updateDto.TransactionDate;
                transaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetTransactionByIdAsync(id, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<bool> DeleteTransactionAsync(long id, long userId)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null)
                {
                    return false;
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<TransactionSummaryDto> GetTransactionSummaryAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Transactions.Where(t => t.UserId == userId);

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= DateOnly.FromDateTime(startDate.Value));
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate <= DateOnly.FromDateTime(endDate.Value));
                }

                var totalIncome = await query.Where(t => t.Type.ToLower() == "income").SumAsync(t => t.Amount);
                var totalExpense = await query.Where(t => t.Type.ToLower() == "expense").SumAsync(t => t.Amount);
                var totalTransactions = await query.CountAsync();
                var incomeTransactions = await query.Where(t => t.Type.ToLower() == "income").CountAsync();
                var expenseTransactions = await query.Where(t => t.Type.ToLower() == "expense").CountAsync();

                var recentTransactions = await GetRecentTransactionsAsync(userId, 10);

                return new TransactionSummaryDto
                {
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    NetAmount = totalIncome - totalExpense,
                    TotalTransactions = totalTransactions,
                    IncomeTransactions = incomeTransactions,
                    ExpenseTransactions = expenseTransactions,
                    RecentTransactions = recentTransactions.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction summary for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetRecentTransactionsAsync(long userId, int count = 10)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .Include(t => t.Category)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(count)
                    .Select(t => new TransactionDto
                    {
                        Id = t.Id,
                        UserId = t.UserId,
                        CategoryId = t.CategoryId,
                        CategoryName = t.Category != null ? t.Category.Name : "Khác",
                        Type = t.Type,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Note = t.Note,
                        TransactionDate = t.TransactionDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    })
                    .ToListAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent transactions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetTotalIncomeAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Transactions.Where(t => t.UserId == userId && t.Type.ToLower() == "income");

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= DateOnly.FromDateTime(startDate.Value));
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate <= DateOnly.FromDateTime(endDate.Value));
                }

                return await query.SumAsync(t => t.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total income for user {UserId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetTotalExpenseAsync(long userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Transactions.Where(t => t.UserId == userId && t.Type.ToLower() == "expense");

                if (startDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= DateOnly.FromDateTime(startDate.Value));
                }

                if (endDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate <= DateOnly.FromDateTime(endDate.Value));
                }

                return await query.SumAsync(t => t.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total expense for user {UserId}", userId);
                throw;
            }
        }
    }
}
