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
    Task<TransactionResponseDto> TransferMoneyAsync(long userId, TransferMoneyDto dto);
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
        {
            // Include transactions where this account is the source OR the destination (PairedAccount)
            query = query.Where(t => t.AccountId == filter.AccountId.Value || (t.PairedAccountId == filter.AccountId.Value && t.TransactionType == 3));
        }

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

        // Pass the filtered AccountId context if accessible, to map descriptions correctly?
        // Actually MapToResponseDto doesn't know which account we are viewing it from.
        // We can infer it if we pass the accountId of interest, but standard usage might be general list.
        // For now, let's keep it simple or guess based on logic.
        
        return transactions.Select(t => MapToResponseDto(t, filter.AccountId)).ToList();
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
                
            // Check sufficient funds in source
            if (account.CurrentBalance < dto.Amount)
            {
                 throw new InvalidOperationException($"Insufficient funds in source wallet '{account.Name}'. Available: {account.CurrentBalance}, Required: {dto.Amount}");
            }

            // Update paired account balance for transfer
            pairedAccount.CurrentBalance += dto.Amount;
            pairedAccount.UpdatedAt = DateTime.UtcNow;
            _context.Accounts.Update(pairedAccount);
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

        // Handle Recurring Transaction
        if (dto.IsRecurring && dto.RecurringInterval > 0)
        {
            var scheduledTransaction = new ScheduledTransaction
            {
                UserId = userId,
                AccountId = dto.AccountId,
                CategoryId = dto.CategoryId,
                TransactionType = dto.TransactionType,
                Amount = dto.Amount,
                Frequency = dto.RecurringFrequency ?? "Monthly", // Default
                Interval = dto.RecurringInterval.Value,
                StartDate = DateOnly.FromDateTime(dto.TransactionDate),
                EndDate = dto.RecurringEndDate.HasValue ? DateOnly.FromDateTime(dto.RecurringEndDate.Value) : null,
                NextRunDate = DateOnly.FromDateTime(dto.TransactionDate).AddMonths(dto.RecurringInterval.Value), // Simple logic for now
                Note = dto.Note,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            // Adjust NextRunDate logic based on Frequency properly
            if (scheduledTransaction.Frequency.Equals("Daily", StringComparison.OrdinalIgnoreCase))
                scheduledTransaction.NextRunDate = scheduledTransaction.StartDate.AddDays(scheduledTransaction.Interval);
            else if (scheduledTransaction.Frequency.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
                scheduledTransaction.NextRunDate = scheduledTransaction.StartDate.AddDays(7 * scheduledTransaction.Interval);
            else if (scheduledTransaction.Frequency.Equals("Monthly", StringComparison.OrdinalIgnoreCase))
                scheduledTransaction.NextRunDate = scheduledTransaction.StartDate.AddMonths(scheduledTransaction.Interval);
            else if (scheduledTransaction.Frequency.Equals("Yearly", StringComparison.OrdinalIgnoreCase))
                scheduledTransaction.NextRunDate = scheduledTransaction.StartDate.AddYears(scheduledTransaction.Interval);
            
             _context.ScheduledTransactions.Add(scheduledTransaction);
        }

        // Update account balance
        if (transaction.TransactionType == 1) // Income
        {
            account.CurrentBalance += transaction.Amount;
        }
        else if (transaction.TransactionType == 2) // Expense
        {
            account.CurrentBalance -= transaction.Amount;
        }
        else if (transaction.TransactionType == 3) // Transfer (Out)
        {
            account.CurrentBalance -= transaction.Amount;
        }

        account.UpdatedAt = DateTime.UtcNow;
        _context.Accounts.Update(account);

        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(transaction).Reference(t => t.Account).LoadAsync();
        await _context.Entry(transaction).Reference(t => t.Category).LoadAsync();
        if (transaction.PairedAccountId.HasValue)
            await _context.Entry(transaction).Reference(t => t.PairedAccount).LoadAsync();

        return MapToResponseDto(transaction);
    }
    
    public async Task<TransactionResponseDto> TransferMoneyAsync(long userId, TransferMoneyDto dto)
    {
         // Basic validations
         if (dto.SourceAccountId == dto.TargetAccountId)
             throw new InvalidOperationException("Cannot transfer to the same wallet.");

         // Validate OTP if needed (mock)
         if (!string.IsNullOrEmpty(dto.OtpCode) && dto.OtpCode != "1234") // Simple mock
             throw new InvalidOperationException("Invalid OTP code.");

         // Daily Limit Check
         var today = DateTime.UtcNow.Date;
         var todaysTransfers = await _context.Transactions
             .Where(t => t.UserId == userId && t.TransactionType == 3 && t.TransactionDate >= today)
             .ToListAsync();
         
         if (todaysTransfers.Count >= 20)
             throw new InvalidOperationException("Daily transfer limit reached (20 transactions).");
             
         var dailyTotal = todaysTransfers.Sum(t => t.Amount);
         if (dailyTotal + dto.Amount > 100000000) // 100M limit
             throw new InvalidOperationException("Daily transfer amount limit reached (100,000,000 VND).");

         var createDto = new CreateTransactionDto
         {
             AccountId = dto.SourceAccountId,
             PairedAccountId = dto.TargetAccountId,
             Amount = dto.Amount,
             Currency = "VND", // Default for now
             TransactionDate = DateTime.UtcNow,
             TransactionType = 3, // Transfer
             Note = dto.Note ?? "Transfer"
         };

         return await CreateTransactionAsync(userId, createDto);
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
        {
            // Revert old amount
            if (transaction.TransactionType == 1) // Income
            {
                transaction.Account.CurrentBalance -= transaction.Amount;
            }
            else if (transaction.TransactionType == 2) // Expense
            {
                transaction.Account.CurrentBalance += transaction.Amount;
            }
            else if (transaction.TransactionType == 3) // Transfer
            {
                transaction.Account.CurrentBalance += transaction.Amount;
                if (transaction.PairedAccount != null)
                {
                    transaction.PairedAccount.CurrentBalance -= transaction.Amount;
                }
            }

            // Apply new amount
            transaction.Amount = dto.Amount.Value;

            if (transaction.TransactionType == 1) // Income
            {
                transaction.Account.CurrentBalance += transaction.Amount;
            }
            else if (transaction.TransactionType == 2) // Expense
            {
                transaction.Account.CurrentBalance -= transaction.Amount;
            }
            else if (transaction.TransactionType == 3) // Transfer
            {
                transaction.Account.CurrentBalance -= transaction.Amount;
                if (transaction.PairedAccount != null)
                {
                    transaction.PairedAccount.CurrentBalance += transaction.Amount;
                    transaction.PairedAccount.UpdatedAt = DateTime.UtcNow;
                    _context.Accounts.Update(transaction.PairedAccount);
                }
            }
            
            transaction.Account.UpdatedAt = DateTime.UtcNow;
            _context.Accounts.Update(transaction.Account);
        }

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
            .Include(t => t.Account)
            .Include(t => t.PairedAccount)
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

        // Revert balance
        if (transaction.TransactionType == 1) // Income
        {
            transaction.Account.CurrentBalance -= transaction.Amount;
        }
        else if (transaction.TransactionType == 2) // Expense
        {
            transaction.Account.CurrentBalance += transaction.Amount;
        }
        else if (transaction.TransactionType == 3) // Transfer
        {
            transaction.Account.CurrentBalance += transaction.Amount;
            
            if (transaction.PairedAccount != null)
            {
                transaction.PairedAccount.CurrentBalance -= transaction.Amount;
                transaction.PairedAccount.UpdatedAt = DateTime.UtcNow;
                _context.Accounts.Update(transaction.PairedAccount);
            }
        }

        transaction.Account.UpdatedAt = DateTime.UtcNow;
        _context.Accounts.Update(transaction.Account);

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

        return transactions.Select(t => MapToResponseDto(t)).ToList();
    }

    // Helper Methods

    private TransactionResponseDto MapToResponseDto(Transaction transaction, long? contextAccountId = null)
    {
        // Generate intelligent description: CategoryName > Note > Default based on type
        string description;
        bool isIncomingTransfer = transaction.TransactionType == 3 && transaction.PairedAccountId == contextAccountId;
        // If contextAccountId is null, we can't easily determine direction if we just look at the list, 
        // but for a general list we might assume standard display. 
        // However, if the row is fetched because PairedAccountId matched, we should probably handle it.
        // But since we selected the transaction row which has AccountId vs PairedAccountId, 
        // the transaction row ITSELF has a main AccountId. 
        // If we want to show it as "Incoming" we need to know if we are viewing it from the perspective of PairedAccount.
        
        if (contextAccountId.HasValue && transaction.PairedAccountId == contextAccountId && transaction.TransactionType == 3)
        {
             description = $"Nhận tiền từ {transaction.Account?.Name ?? "ví khác"}";
        }
        else if (!string.IsNullOrWhiteSpace(transaction.Category?.Name))
        {
            description = transaction.Category.Name;
        }
        else if (!string.IsNullOrWhiteSpace(transaction.Note))
        {
            description = transaction.Note;
        }
        else
        {
            // Default description based on transaction type
            description = transaction.TransactionType switch
            {
                1 => "Thu nhập",
                2 => "Chi tiêu",
                3 => $"Chuyển khoản đến {transaction.PairedAccount?.Name ?? "ví khác"}",
                _ => "Giao dịch"
            };
        }

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
            UpdatedAt = transaction.UpdatedAt,
            Description = description
        };
    }

    private string GetTransactionTypeDisplay(int transactionType)
    {
        return transactionType switch
        {
            1 => "Thu nhập",
            2 => "Chi tiêu",
            3 => "Chuyển tiền",
            _ => "Không xác định"
        };
    }
}
