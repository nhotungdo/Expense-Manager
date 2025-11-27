using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing transactions (Income, Expense, Transfer)
/// Handles CRUD operations, filtering, and balance updates
/// </summary>
public interface ITransactionService
{
    Task<TransactionResponseDto?> GetTransactionByIdAsync(long transactionId, long userId);
    Task<List<TransactionResponseDto>> GetUserTransactionsAsync(long userId, TransactionFilterDto filter);
    Task<TransactionResponseDto> CreateTransactionAsync(long userId, CreateTransactionDto dto);
    Task<TransactionResponseDto> UpdateTransactionAsync(long userId, UpdateTransactionDto dto);
    Task<bool> DeleteTransactionAsync(long transactionId, long userId);
    Task<decimal> GetAccountBalanceFromTransactionsAsync(long accountId);
    Task<List<TransactionResponseDto>> GetRecentTransactionsAsync(long userId, int count = 10);
}

public class TransactionService : ITransactionService
{
    private readonly ExpenseManagerContext _context;

    public TransactionService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific transaction by ID
    /// </summary>
    public async Task<TransactionResponseDto?> GetTransactionByIdAsync(long transactionId, long userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.PairedAccount)
            .Where(t => t.Id == transactionId && t.UserId == userId)
            .FirstOrDefaultAsync();

        if (transaction == null)
            return null;

        return MapToResponseDto(transaction);
    }

    /// <summary>
    /// Get transactions with filtering and pagination
    /// </summary>
    public async Task<List<TransactionResponseDto>> GetUserTransactionsAsync(long userId, TransactionFilterDto filter)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.PairedAccount)
            .Where(t => t.UserId == userId);

        // Apply filters
        if (filter.AccountId.HasValue)
            query = query.Where(t => t.AccountId == filter.AccountId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.TransactionType.HasValue)
            query = query.Where(t => t.TransactionType == filter.TransactionType.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value);

        if (filter.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            query = query.Where(t => t.Note != null && t.Note.Contains(filter.SearchText));

        // Apply pagination
        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return transactions.Select(MapToResponseDto).ToList();
    }

    /// <summary>
    /// Create a new transaction (Income, Expense, or Transfer)
    /// </summary>
    public async Task<TransactionResponseDto> CreateTransactionAsync(long userId, CreateTransactionDto dto)
    {
        // Verify account belongs to user
        var account = await _context.Accounts
            .Where(a => a.Id == dto.AccountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            throw new InvalidOperationException("Account not found or you don't have permission");

        // For transfer transactions, verify paired account
        if (dto.TransactionType == 3 && dto.PairedAccountId.HasValue)
        {
            var pairedAccount = await _context.Accounts
                .Where(a => a.Id == dto.PairedAccountId.Value && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (pairedAccount == null)
                throw new InvalidOperationException("Paired account not found or you don't have permission");
        }

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            TransactionType = dto.TransactionType,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Note = dto.Note,
            TransactionDate = dto.TransactionDate,
            PairedAccountId = dto.PairedAccountId,
            AttachmentUrl = dto.AttachmentUrl,
            OcrText = dto.OcrText,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        // For transfer transactions, create paired transaction
        if (dto.TransactionType == 3 && dto.PairedAccountId.HasValue)
        {
            var pairedTransaction = new Transaction
            {
                UserId = userId,
                AccountId = dto.PairedAccountId.Value,
                CategoryId = dto.CategoryId,
                TransactionType = 3, // Transfer
                Amount = dto.Amount,
                Currency = dto.Currency,
                Note = dto.Note,
                TransactionDate = dto.TransactionDate,
                PairedAccountId = dto.AccountId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(pairedTransaction);
            await _context.SaveChangesAsync();

            // Link the paired transactions
            transaction.PairedTransactionId = pairedTransaction.Id;
            pairedTransaction.PairedTransactionId = transaction.Id;
        }

        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(transaction).Reference(t => t.Account).LoadAsync();
        await _context.Entry(transaction).Reference(t => t.Category).LoadAsync();
        if (transaction.PairedAccountId.HasValue)
            await _context.Entry(transaction).Reference(t => t.PairedAccount).LoadAsync();

        return MapToResponseDto(transaction);
    }

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    public async Task<TransactionResponseDto> UpdateTransactionAsync(long userId, UpdateTransactionDto dto)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.PairedAccount)
            .Where(t => t.Id == dto.Id && t.UserId == userId)
            .FirstOrDefaultAsync();

        if (transaction == null)
            throw new InvalidOperationException("Transaction not found");

        // Update fields
        if (dto.CategoryId.HasValue)
            transaction.CategoryId = dto.CategoryId.Value;

        if (dto.Amount.HasValue)
            transaction.Amount = dto.Amount.Value;

        if (!string.IsNullOrWhiteSpace(dto.Note))
            transaction.Note = dto.Note;

        if (dto.TransactionDate.HasValue)
            transaction.TransactionDate = dto.TransactionDate.Value;

        if (!string.IsNullOrWhiteSpace(dto.AttachmentUrl))
            transaction.AttachmentUrl = dto.AttachmentUrl;

        transaction.UpdatedAt = DateTime.UtcNow;

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();

        return MapToResponseDto(transaction);
    }

    /// <summary>
    /// Delete a transaction
    /// </summary>
    public async Task<bool> DeleteTransactionAsync(long transactionId, long userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.PairedTransaction)
            .Where(t => t.Id == transactionId && t.UserId == userId)
            .FirstOrDefaultAsync();

        if (transaction == null)
            return false;

        // If it's a transfer, delete both transactions
        if (transaction.TransactionType == 3 && transaction.PairedTransactionId.HasValue)
        {
            var pairedTransaction = await _context.Transactions
                .FindAsync(transaction.PairedTransactionId.Value);

            if (pairedTransaction != null)
                _context.Transactions.Remove(pairedTransaction);
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Calculate account balance from transactions
    /// </summary>
    public async Task<decimal> GetAccountBalanceFromTransactionsAsync(long accountId)
    {
        var income = await _context.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionType == 1)
            .SumAsync(t => t.Amount);

        var expense = await _context.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionType == 2)
            .SumAsync(t => t.Amount);

        var transferIn = await _context.Transactions
            .Where(t => t.PairedAccountId == accountId && t.TransactionType == 3)
            .SumAsync(t => t.Amount);

        var transferOut = await _context.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionType == 3)
            .SumAsync(t => t.Amount);

        return income - expense + transferIn - transferOut;
    }

    /// <summary>
    /// Get recent transactions for dashboard
    /// </summary>
    public async Task<List<TransactionResponseDto>> GetRecentTransactionsAsync(long userId, int count = 10)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.PairedAccount)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();

        return transactions.Select(MapToResponseDto).ToList();
    }

    // Helper Methods

    private TransactionResponseDto MapToResponseDto(Transaction transaction)
    {
        return new TransactionResponseDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            AccountId = transaction.AccountId,
            AccountName = transaction.Account?.Name ?? "Unknown",
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category?.Name,
            CategoryIcon = transaction.Category?.Icon,
            CategoryColor = transaction.Category?.Color,
            TransactionType = transaction.TransactionType,
            TransactionTypeDisplay = GetTransactionTypeDisplay(transaction.TransactionType),
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Note = transaction.Note,
            TransactionDate = transaction.TransactionDate,
            PairedAccountId = transaction.PairedAccountId,
            PairedAccountName = transaction.PairedAccount?.Name,
            PairedTransactionId = transaction.PairedTransactionId,
            AttachmentUrl = transaction.AttachmentUrl,
            OcrText = transaction.OcrText,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }

    private string GetTransactionTypeDisplay(int transactionType)
    {
        return transactionType switch
        {
            1 => "Income",
            2 => "Expense",
            3 => "Transfer",
            _ => "Unknown"
        };
    }
}
