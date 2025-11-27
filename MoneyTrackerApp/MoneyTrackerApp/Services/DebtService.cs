using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing debts with payment tracking and interest calculation
/// Handles both "I owe them" and "They owe me" scenarios
/// </summary>
public interface IDebtService
{
    Task<DebtResponseDto?> GetDebtByIdAsync(long debtId, long userId);
    Task<List<DebtResponseDto>> GetUserDebtsAsync(long userId, int? debtType = null);
    Task<DebtSummaryDto> GetDebtSummaryAsync(long userId);
    Task<DebtResponseDto> CreateDebtAsync(long userId, CreateDebtDto dto);
    Task<DebtResponseDto> UpdateDebtAsync(long userId, UpdateDebtDto dto);
    Task<bool> DeleteDebtAsync(long debtId, long userId);
    Task<DebtResponseDto> RecordPaymentAsync(long userId, RecordDebtPaymentDto dto);
    Task<decimal> CalculateInterestAsync(long debtId);
}

public class DebtService : IDebtService
{
    private readonly ExpenseManagerContext _context;

    public DebtService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific debt by ID
    /// </summary>
    public async Task<DebtResponseDto?> GetDebtByIdAsync(long debtId, long userId)
    {
        var debt = await _context.Debts
            .Include(d => d.DebtPayments)
            .Where(d => d.Id == debtId && d.UserId == userId)
            .FirstOrDefaultAsync();

        if (debt == null)
            return null;

        return await MapToResponseDtoAsync(debt);
    }

    /// <summary>
    /// Get all debts for a user
    /// </summary>
    public async Task<List<DebtResponseDto>> GetUserDebtsAsync(long userId, int? debtType = null)
    {
        var query = _context.Debts
            .Include(d => d.DebtPayments)
            .Where(d => d.UserId == userId);

        if (debtType.HasValue)
            query = query.Where(d => d.DebtType == debtType.Value);

        var debts = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var result = new List<DebtResponseDto>();
        foreach (var debt in debts)
        {
            result.Add(await MapToResponseDtoAsync(debt));
        }

        return result;
    }

    /// <summary>
    /// Get debt summary with totals
    /// </summary>
    public async Task<DebtSummaryDto> GetDebtSummaryAsync(long userId)
    {
        var allDebts = await GetUserDebtsAsync(userId);
        var iOweThem = allDebts.Where(d => d.DebtType == 1).ToList();
        var theyOweMe = allDebts.Where(d => d.DebtType == 2).ToList();

        var totalIOwe = iOweThem.Sum(d => d.RemainingAmount);
        var totalTheyOweMe = theyOweMe.Sum(d => d.RemainingAmount);
        var netDebt = totalIOwe - totalTheyOweMe;
        var totalInterest = allDebts.Sum(d => d.InterestAmount);

        return new DebtSummaryDto
        {
            TotalDebts = allDebts.Count,
            ActiveDebts = allDebts.Count(d => d.Status == 1),
            TotalIOwe = totalIOwe,
            TotalTheyOweMe = totalTheyOweMe,
            NetDebt = netDebt,
            TotalInterest = totalInterest,
            IOweThem = iOweThem,
            TheyOweMe = theyOweMe
        };
    }

    /// <summary>
    /// Create a new debt
    /// </summary>
    public async Task<DebtResponseDto> CreateDebtAsync(long userId, CreateDebtDto dto)
    {
        var debt = new Debt
        {
            UserId = userId,
            DebtType = dto.DebtType,
            Name = dto.Name,
            PersonName = dto.PersonName,
            InitialAmount = dto.InitialAmount,
            AmountPaid = 0,
            InterestRate = dto.InterestRate,
            StartDate = dto.StartDate,
            DueDate = dto.DueDate,
            Status = 1, // Active
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Debts.Add(debt);
        await _context.SaveChangesAsync();

        return await MapToResponseDtoAsync(debt);
    }

    /// <summary>
    /// Update an existing debt
    /// </summary>
    public async Task<DebtResponseDto> UpdateDebtAsync(long userId, UpdateDebtDto dto)
    {
        var debt = await _context.Debts
            .Include(d => d.DebtPayments)
            .Where(d => d.Id == dto.Id && d.UserId == userId)
            .FirstOrDefaultAsync();

        if (debt == null)
            throw new InvalidOperationException("Debt not found");

        // Update fields
        if (!string.IsNullOrWhiteSpace(dto.Name))
            debt.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.PersonName))
            debt.PersonName = dto.PersonName;

        if (dto.InterestRate.HasValue)
            debt.InterestRate = dto.InterestRate.Value;

        if (dto.DueDate.HasValue)
            debt.DueDate = dto.DueDate.Value;

        if (dto.Status.HasValue)
            debt.Status = dto.Status.Value;

        debt.UpdatedAt = DateTime.UtcNow;

        _context.Debts.Update(debt);
        await _context.SaveChangesAsync();

        return await MapToResponseDtoAsync(debt);
    }

    /// <summary>
    /// Delete a debt
    /// </summary>
    public async Task<bool> DeleteDebtAsync(long debtId, long userId)
    {
        var debt = await _context.Debts
            .Include(d => d.DebtPayments)
            .Where(d => d.Id == debtId && d.UserId == userId)
            .FirstOrDefaultAsync();

        if (debt == null)
            return false;

        // Remove associated payments
        _context.DebtPayments.RemoveRange(debt.DebtPayments);

        // Remove debt
        _context.Debts.Remove(debt);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Record a debt payment
    /// </summary>
    public async Task<DebtResponseDto> RecordPaymentAsync(long userId, RecordDebtPaymentDto dto)
    {
        var debt = await _context.Debts
            .Include(d => d.DebtPayments)
            .Where(d => d.Id == dto.DebtId && d.UserId == userId)
            .FirstOrDefaultAsync();

        if (debt == null)
            throw new InvalidOperationException("Debt not found");

        // Verify transaction exists and belongs to user
        var transaction = await _context.Transactions
            .Where(t => t.Id == dto.TransactionId && t.UserId == userId)
            .FirstOrDefaultAsync();

        if (transaction == null)
            throw new InvalidOperationException("Transaction not found");

        // Create debt payment
        var payment = new DebtPayment
        {
            DebtId = dto.DebtId,
            TransactionId = dto.TransactionId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate,
            Note = dto.Note
        };

        _context.DebtPayments.Add(payment);

        // Update debt amount paid
        debt.AmountPaid += dto.Amount;

        // Calculate interest
        var interestAmount = await CalculateInterestAsync(debt.Id);
        var totalWithInterest = debt.InitialAmount + interestAmount;

        // Update status
        if (debt.AmountPaid >= totalWithInterest)
        {
            debt.Status = 3; // Fully Paid
        }
        else if (debt.AmountPaid > 0)
        {
            debt.Status = 2; // Partially Paid
        }

        debt.UpdatedAt = DateTime.UtcNow;

        _context.Debts.Update(debt);
        await _context.SaveChangesAsync();

        return await MapToResponseDtoAsync(debt);
    }

    /// <summary>
    /// Calculate simple interest for a debt
    /// </summary>
    public async Task<decimal> CalculateInterestAsync(long debtId)
    {
        var debt = await _context.Debts.FindAsync(debtId);
        if (debt == null || debt.InterestRate == 0)
            return 0;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysPassed = today.DayNumber - debt.StartDate.DayNumber;
        var years = daysPassed / 365.0m;

        // Simple Interest Formula: I = P * R * T
        // P = Principal (InitialAmount)
        // R = Rate (InterestRate / 100)
        // T = Time in years
        var interest = debt.InitialAmount * (debt.InterestRate / 100) * years;

        return interest;
    }

    // Helper Methods

    private async Task<DebtResponseDto> MapToResponseDtoAsync(Debt debt)
    {
        var interestAmount = await CalculateInterestAsync(debt.Id);
        var totalWithInterest = debt.InitialAmount + interestAmount;
        var remainingAmount = totalWithInterest - debt.AmountPaid;
        var percentagePaid = totalWithInterest > 0 ? (debt.AmountPaid / totalWithInterest) * 100 : 0;

        int? daysRemaining = null;
        bool isOverdue = false;

        if (debt.DueDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            daysRemaining = debt.DueDate.Value.DayNumber - today.DayNumber;
            isOverdue = daysRemaining < 0 && remainingAmount > 0;
        }

        return new DebtResponseDto
        {
            Id = debt.Id,
            UserId = debt.UserId,
            DebtType = debt.DebtType,
            DebtTypeDisplay = debt.DebtType == 1 ? "I Owe Them" : "They Owe Me",
            Name = debt.Name,
            PersonName = debt.PersonName,
            InitialAmount = debt.InitialAmount,
            AmountPaid = debt.AmountPaid,
            RemainingAmount = remainingAmount > 0 ? remainingAmount : 0,
            InterestRate = debt.InterestRate,
            InterestAmount = interestAmount,
            TotalWithInterest = totalWithInterest,
            StartDate = debt.StartDate,
            DueDate = debt.DueDate,
            DaysRemaining = daysRemaining,
            IsOverdue = isOverdue,
            Status = debt.Status,
            StatusDisplay = GetStatusDisplay(debt.Status),
            PercentagePaid = percentagePaid,
            CreatedAt = debt.CreatedAt,
            UpdatedAt = debt.UpdatedAt,
            Payments = debt.DebtPayments.Select(dp => new DebtPaymentDto
            {
                Id = dp.Id,
                DebtId = dp.DebtId,
                TransactionId = dp.TransactionId,
                Amount = dp.Amount,
                PaymentDate = dp.PaymentDate,
                Note = dp.Note
            }).ToList()
        };
    }

    private string GetStatusDisplay(int status)
    {
        return status switch
        {
            1 => "Active",
            2 => "Partially Paid",
            3 => "Fully Paid",
            4 => "Cancelled",
            _ => "Unknown"
        };
    }
}
