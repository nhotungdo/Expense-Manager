using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing savings goals with progress tracking
/// Handles goal CRUD, savings transactions, and completion tracking
/// </summary>
public interface ISavingsGoalService
{
    Task<SavingsGoalResponseDto?> GetSavingsGoalByIdAsync(long goalId, long userId);
    Task<List<SavingsGoalResponseDto>> GetUserSavingsGoalsAsync(long userId, bool activeOnly = true);
    Task<SavingsSummaryDto> GetSavingsSummaryAsync(long userId);
    Task<SavingsGoalResponseDto> CreateSavingsGoalAsync(long userId, CreateSavingsGoalDto dto);
    Task<SavingsGoalResponseDto> UpdateSavingsGoalAsync(long userId, UpdateSavingsGoalDto dto);
    Task<bool> DeleteSavingsGoalAsync(long goalId, long userId);
    Task<SavingsGoalResponseDto> AddToSavingsAsync(long userId, AddToSavingsDto dto);
    Task<bool> CompleteSavingsGoalAsync(long goalId, long userId);
}

public class SavingsGoalService : ISavingsGoalService
{
    private readonly ExpenseManagerContext _context;

    public SavingsGoalService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific savings goal by ID
    /// </summary>
    public async Task<SavingsGoalResponseDto?> GetSavingsGoalByIdAsync(long goalId, long userId)
    {
        var goal = await _context.SavingsGoals
            .Include(sg => sg.SavingsTransactions)
            .Where(sg => sg.Id == goalId && sg.UserId == userId)
            .FirstOrDefaultAsync();

        if (goal == null)
            return null;

        return MapToResponseDto(goal);
    }

    /// <summary>
    /// Get all savings goals for a user
    /// </summary>
    public async Task<List<SavingsGoalResponseDto>> GetUserSavingsGoalsAsync(long userId, bool activeOnly = true)
    {
        var query = _context.SavingsGoals
            .Include(sg => sg.SavingsTransactions)
            .Where(sg => sg.UserId == userId);

        if (activeOnly)
            query = query.Where(sg => sg.Status == 1); // Active only

        var goals = await query
            .OrderByDescending(sg => sg.CreatedAt)
            .ToListAsync();

        return goals.Select(MapToResponseDto).ToList();
    }

    /// <summary>
    /// Get savings summary with totals
    /// </summary>
    public async Task<SavingsSummaryDto> GetSavingsSummaryAsync(long userId)
    {
        var goals = await GetUserSavingsGoalsAsync(userId, activeOnly: false);

        var totalTargetAmount = goals.Sum(g => g.TargetAmount);
        var totalSavedAmount = goals.Sum(g => g.CurrentAmount);
        var totalRemainingAmount = goals.Sum(g => g.RemainingAmount);
        var overallPercentage = totalTargetAmount > 0 ? (totalSavedAmount / totalTargetAmount) * 100 : 0;

        return new SavingsSummaryDto
        {
            TotalGoals = goals.Count,
            ActiveGoals = goals.Count(g => g.Status == 1),
            CompletedGoals = goals.Count(g => g.Status == 2),
            TotalTargetAmount = totalTargetAmount,
            TotalSavedAmount = totalSavedAmount,
            TotalRemainingAmount = totalRemainingAmount,
            OverallPercentage = overallPercentage,
            Goals = goals
        };
    }

    /// <summary>
    /// Create a new savings goal
    /// </summary>
    public async Task<SavingsGoalResponseDto> CreateSavingsGoalAsync(long userId, CreateSavingsGoalDto dto)
    {
        var goal = new SavingsGoal
        {
            UserId = userId,
            Name = dto.Name,
            TargetAmount = dto.TargetAmount,
            CurrentAmount = 0,
            TargetDate = dto.TargetDate,
            Icon = dto.Icon,
            Color = dto.Color,
            Status = 1, // Active
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SavingsGoals.Add(goal);
        await _context.SaveChangesAsync();

        return MapToResponseDto(goal);
    }

    /// <summary>
    /// Update an existing savings goal
    /// </summary>
    public async Task<SavingsGoalResponseDto> UpdateSavingsGoalAsync(long userId, UpdateSavingsGoalDto dto)
    {
        var goal = await _context.SavingsGoals
            .Include(sg => sg.SavingsTransactions)
            .Where(sg => sg.Id == dto.Id && sg.UserId == userId)
            .FirstOrDefaultAsync();

        if (goal == null)
            throw new InvalidOperationException("Savings goal not found");

        // Update fields
        if (!string.IsNullOrWhiteSpace(dto.Name))
            goal.Name = dto.Name;

        if (dto.TargetAmount.HasValue)
            goal.TargetAmount = dto.TargetAmount.Value;

        if (dto.TargetDate.HasValue)
            goal.TargetDate = dto.TargetDate.Value;

        if (!string.IsNullOrWhiteSpace(dto.Icon))
            goal.Icon = dto.Icon;

        if (!string.IsNullOrWhiteSpace(dto.Color))
            goal.Color = dto.Color;

        if (dto.Status.HasValue)
            goal.Status = dto.Status.Value;

        goal.UpdatedAt = DateTime.UtcNow;

        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();

        return MapToResponseDto(goal);
    }

    /// <summary>
    /// Delete a savings goal
    /// </summary>
    public async Task<bool> DeleteSavingsGoalAsync(long goalId, long userId)
    {
        var goal = await _context.SavingsGoals
            .Include(sg => sg.SavingsTransactions)
            .Where(sg => sg.Id == goalId && sg.UserId == userId)
            .FirstOrDefaultAsync();

        if (goal == null)
            return false;

        // Remove associated savings transactions
        _context.SavingsTransactions.RemoveRange(goal.SavingsTransactions);

        // Remove goal
        _context.SavingsGoals.Remove(goal);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Add money to a savings goal
    /// </summary>
    public async Task<SavingsGoalResponseDto> AddToSavingsAsync(long userId, AddToSavingsDto dto)
    {
        var goal = await _context.SavingsGoals
            .Include(sg => sg.SavingsTransactions)
            .Where(sg => sg.Id == dto.SavingsGoalId && sg.UserId == userId)
            .FirstOrDefaultAsync();

        if (goal == null)
            throw new InvalidOperationException("Savings goal not found");

        // Verify transaction exists and belongs to user
        var transaction = await _context.Transactions
            .Where(t => t.Id == dto.TransactionId && t.UserId == userId)
            .FirstOrDefaultAsync();

        if (transaction == null)
            throw new InvalidOperationException("Transaction not found");

        // Create savings transaction
        var savingsTransaction = new SavingsTransaction
        {
            SavingsGoalId = dto.SavingsGoalId,
            TransactionId = dto.TransactionId,
            Amount = dto.Amount,
            TransactionDate = transaction.TransactionDate,
            Note = dto.Note
        };

        _context.SavingsTransactions.Add(savingsTransaction);

        // Update goal current amount
        goal.CurrentAmount += dto.Amount;

        // Check if goal is completed
        if (goal.CurrentAmount >= goal.TargetAmount && goal.Status == 1)
        {
            goal.Status = 2; // Completed
        }

        goal.UpdatedAt = DateTime.UtcNow;

        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();

        return MapToResponseDto(goal);
    }

    /// <summary>
    /// Mark a savings goal as completed
    /// </summary>
    public async Task<bool> CompleteSavingsGoalAsync(long goalId, long userId)
    {
        var goal = await _context.SavingsGoals
            .Where(sg => sg.Id == goalId && sg.UserId == userId)
            .FirstOrDefaultAsync();

        if (goal == null)
            return false;

        goal.Status = 2; // Completed
        goal.UpdatedAt = DateTime.UtcNow;

        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();

        return true;
    }

    // Helper Methods

    private SavingsGoalResponseDto MapToResponseDto(SavingsGoal goal)
    {
        var remainingAmount = goal.TargetAmount - goal.CurrentAmount;
        var percentageCompleted = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount) * 100 : 0;
        var isCompleted = goal.CurrentAmount >= goal.TargetAmount;

        int? daysRemaining = null;
        bool isOverdue = false;

        if (goal.TargetDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            daysRemaining = goal.TargetDate.Value.DayNumber - today.DayNumber;
            isOverdue = daysRemaining < 0 && !isCompleted;
        }

        return new SavingsGoalResponseDto
        {
            Id = goal.Id,
            UserId = goal.UserId,
            Name = goal.Name,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            RemainingAmount = remainingAmount > 0 ? remainingAmount : 0,
            PercentageCompleted = percentageCompleted,
            TargetDate = goal.TargetDate,
            DaysRemaining = daysRemaining,
            Icon = goal.Icon,
            Color = goal.Color,
            Status = goal.Status,
            StatusDisplay = GetStatusDisplay(goal.Status),
            IsCompleted = isCompleted,
            IsOverdue = isOverdue,
            CreatedAt = goal.CreatedAt,
            UpdatedAt = goal.UpdatedAt,
            Transactions = goal.SavingsTransactions.Select(st => new SavingsTransactionDto
            {
                Id = st.Id,
                SavingsGoalId = st.SavingsGoalId,
                TransactionId = st.TransactionId,
                Amount = st.Amount,
                TransactionDate = st.TransactionDate,
                Note = st.Note
            }).ToList()
        };
    }

    private string GetStatusDisplay(int status)
    {
        return status switch
        {
            1 => "Active",
            2 => "Completed",
            3 => "Cancelled",
            _ => "Unknown"
        };
    }
}
