using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing scheduled/recurring transactions
/// Handles CRUD operations and automatic transaction execution
/// </summary>
public interface IScheduledTransactionService
{
    Task<ScheduledTransactionResponseDto?> GetScheduledTransactionByIdAsync(long id, long userId);
    Task<List<ScheduledTransactionResponseDto>> GetUserScheduledTransactionsAsync(long userId, bool activeOnly = true);
    Task<ScheduledTransactionResponseDto> CreateScheduledTransactionAsync(long userId, CreateScheduledTransactionDto dto);
    Task<ScheduledTransactionResponseDto> UpdateScheduledTransactionAsync(long userId, UpdateScheduledTransactionDto dto);
    Task<bool> DeleteScheduledTransactionAsync(long id, long userId);
    Task<bool> ToggleScheduledTransactionAsync(long id, long userId, bool isActive);
    Task<List<ScheduledTransactionResponseDto>> GetDueScheduledTransactionsAsync();
    Task ExecuteScheduledTransactionAsync(long id);
}

public class ScheduledTransactionService : IScheduledTransactionService
{
    private readonly ExpenseManagerContext _context;
    private readonly ITransactionService _transactionService;

    public ScheduledTransactionService(ExpenseManagerContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    /// <summary>
    /// Get a specific scheduled transaction
    /// </summary>
    public async Task<ScheduledTransactionResponseDto?> GetScheduledTransactionByIdAsync(long id, long userId)
    {
        var scheduled = await _context.ScheduledTransactions
            .Include(st => st.Account)
            .Include(st => st.Category)
            .Where(st => st.Id == id && st.UserId == userId)
            .FirstOrDefaultAsync();

        if (scheduled == null)
            return null;

        return MapToResponseDto(scheduled);
    }

    /// <summary>
    /// Get all scheduled transactions for a user
    /// </summary>
    public async Task<List<ScheduledTransactionResponseDto>> GetUserScheduledTransactionsAsync(long userId, bool activeOnly = true)
    {
        var query = _context.ScheduledTransactions
            .Include(st => st.Account)
            .Include(st => st.Category)
            .Where(st => st.UserId == userId);

        if (activeOnly)
            query = query.Where(st => st.IsActive);

        var scheduled = await query
            .OrderBy(st => st.NextRunDate)
            .ToListAsync();

        return scheduled.Select(MapToResponseDto).ToList();
    }

    /// <summary>
    /// Create a new scheduled transaction
    /// </summary>
    public async Task<ScheduledTransactionResponseDto> CreateScheduledTransactionAsync(long userId, CreateScheduledTransactionDto dto)
    {
        // Verify account belongs to user
        var account = await _context.Accounts
            .Where(a => a.Id == dto.AccountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            throw new InvalidOperationException("Account not found or you don't have permission");

        // Calculate next run date
        var nextRunDate = CalculateNextRunDate(dto.StartDate, dto.Frequency, dto.Interval);

        var scheduled = new ScheduledTransaction
        {
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            TransactionType = dto.TransactionType,
            Amount = dto.Amount,
            Frequency = dto.Frequency,
            Interval = dto.Interval,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            NextRunDate = nextRunDate,
            Note = dto.Note,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ScheduledTransactions.Add(scheduled);
        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(scheduled).Reference(st => st.Account).LoadAsync();
        await _context.Entry(scheduled).Reference(st => st.Category).LoadAsync();

        return MapToResponseDto(scheduled);
    }

    /// <summary>
    /// Update an existing scheduled transaction
    /// </summary>
    public async Task<ScheduledTransactionResponseDto> UpdateScheduledTransactionAsync(long userId, UpdateScheduledTransactionDto dto)
    {
        var scheduled = await _context.ScheduledTransactions
            .Include(st => st.Account)
            .Include(st => st.Category)
            .Where(st => st.Id == dto.Id && st.UserId == userId)
            .FirstOrDefaultAsync();

        if (scheduled == null)
            throw new InvalidOperationException("Scheduled transaction not found");

        // Update fields
        if (dto.CategoryId.HasValue)
            scheduled.CategoryId = dto.CategoryId.Value;

        if (dto.Amount.HasValue)
            scheduled.Amount = dto.Amount.Value;

        if (!string.IsNullOrWhiteSpace(dto.Frequency))
            scheduled.Frequency = dto.Frequency;

        if (dto.Interval.HasValue)
            scheduled.Interval = dto.Interval.Value;

        if (dto.EndDate.HasValue)
            scheduled.EndDate = dto.EndDate.Value;

        if (!string.IsNullOrWhiteSpace(dto.Note))
            scheduled.Note = dto.Note;

        if (dto.IsActive.HasValue)
            scheduled.IsActive = dto.IsActive.Value;

        // Recalculate next run date if frequency or interval changed
        if (!string.IsNullOrWhiteSpace(dto.Frequency) || dto.Interval.HasValue)
        {
            scheduled.NextRunDate = CalculateNextRunDate(
                DateOnly.FromDateTime(DateTime.UtcNow),
                scheduled.Frequency,
                scheduled.Interval
            );
        }

        scheduled.UpdatedAt = DateTime.UtcNow;

        _context.ScheduledTransactions.Update(scheduled);
        await _context.SaveChangesAsync();

        return MapToResponseDto(scheduled);
    }

    /// <summary>
    /// Delete a scheduled transaction
    /// </summary>
    public async Task<bool> DeleteScheduledTransactionAsync(long id, long userId)
    {
        var scheduled = await _context.ScheduledTransactions
            .Where(st => st.Id == id && st.UserId == userId)
            .FirstOrDefaultAsync();

        if (scheduled == null)
            return false;

        _context.ScheduledTransactions.Remove(scheduled);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Toggle scheduled transaction active status
    /// </summary>
    public async Task<bool> ToggleScheduledTransactionAsync(long id, long userId, bool isActive)
    {
        var scheduled = await _context.ScheduledTransactions
            .Where(st => st.Id == id && st.UserId == userId)
            .FirstOrDefaultAsync();

        if (scheduled == null)
            return false;

        scheduled.IsActive = isActive;
        scheduled.UpdatedAt = DateTime.UtcNow;

        _context.ScheduledTransactions.Update(scheduled);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Get all scheduled transactions that are due for execution
    /// </summary>
    public async Task<List<ScheduledTransactionResponseDto>> GetDueScheduledTransactionsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var dueTransactions = await _context.ScheduledTransactions
            .Include(st => st.Account)
            .Include(st => st.Category)
            .Where(st => st.IsActive && st.NextRunDate <= today)
            .Where(st => !st.EndDate.HasValue || st.EndDate.Value >= today)
            .ToListAsync();

        return dueTransactions.Select(MapToResponseDto).ToList();
    }

    /// <summary>
    /// Execute a scheduled transaction (create actual transaction)
    /// </summary>
    public async Task ExecuteScheduledTransactionAsync(long id)
    {
        var scheduled = await _context.ScheduledTransactions
            .Include(st => st.Account)
            .Where(st => st.Id == id && st.IsActive)
            .FirstOrDefaultAsync();

        if (scheduled == null)
            throw new InvalidOperationException("Scheduled transaction not found or inactive");

        // Create the actual transaction
        var transactionDto = new CreateTransactionDto
        {
            AccountId = scheduled.AccountId,
            CategoryId = scheduled.CategoryId,
            TransactionType = scheduled.TransactionType,
            Amount = scheduled.Amount,
            Currency = scheduled.Account.Currency,
            Note = $"{scheduled.Note} (Scheduled)",
            TransactionDate = DateTime.UtcNow
        };

        await _transactionService.CreateTransactionAsync(scheduled.UserId, transactionDto);

        // Update next run date
        scheduled.NextRunDate = CalculateNextRunDate(
            scheduled.NextRunDate,
            scheduled.Frequency,
            scheduled.Interval
        );

        // Check if we should deactivate (past end date)
        if (scheduled.EndDate.HasValue && scheduled.NextRunDate > scheduled.EndDate.Value)
        {
            scheduled.IsActive = false;
        }

        scheduled.UpdatedAt = DateTime.UtcNow;

        _context.ScheduledTransactions.Update(scheduled);
        await _context.SaveChangesAsync();
    }

    // Helper Methods

    private DateOnly CalculateNextRunDate(DateOnly currentDate, string frequency, int interval)
    {
        return frequency switch
        {
            "Daily" => currentDate.AddDays(interval),
            "Weekly" => currentDate.AddDays(interval * 7),
            "Monthly" => currentDate.AddMonths(interval),
            "Yearly" => currentDate.AddYears(interval),
            _ => currentDate.AddDays(1)
        };
    }

    private ScheduledTransactionResponseDto MapToResponseDto(ScheduledTransaction scheduled)
    {
        return new ScheduledTransactionResponseDto
        {
            Id = scheduled.Id,
            UserId = scheduled.UserId,
            AccountId = scheduled.AccountId,
            AccountName = scheduled.Account?.Name ?? "Unknown",
            CategoryId = scheduled.CategoryId,
            CategoryName = scheduled.Category?.Name,
            CategoryIcon = scheduled.Category?.Icon,
            CategoryColor = scheduled.Category?.Color,
            TransactionType = scheduled.TransactionType,
            TransactionTypeDisplay = scheduled.TransactionType == 1 ? "Thu nhập" : "Chi tiêu",
            Amount = scheduled.Amount,
            Frequency = scheduled.Frequency,
            Interval = scheduled.Interval,
            StartDate = scheduled.StartDate,
            EndDate = scheduled.EndDate,
            NextRunDate = scheduled.NextRunDate,
            Note = scheduled.Note,
            IsActive = scheduled.IsActive,
            CreatedAt = scheduled.CreatedAt,
            UpdatedAt = scheduled.UpdatedAt
        };
    }
}
