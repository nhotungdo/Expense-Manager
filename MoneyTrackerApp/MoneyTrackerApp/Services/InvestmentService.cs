using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing investment portfolio with P/L calculation
/// Handles Gold, Stocks, Crypto, and other asset types
/// </summary>
public interface IInvestmentService
{
    Task<InvestmentResponseDto?> GetInvestmentByIdAsync(long investmentId, long userId);
    Task<List<InvestmentResponseDto>> GetUserInvestmentsAsync(long userId, string? assetType = null);
    Task<InvestmentPortfolioDto> GetPortfolioSummaryAsync(long userId);
    Task<InvestmentResponseDto> CreateInvestmentAsync(long userId, CreateInvestmentDto dto);
    Task<InvestmentResponseDto> UpdateInvestmentAsync(long userId, UpdateInvestmentDto dto);
    Task<InvestmentResponseDto> UpdateMarketPriceAsync(long userId, UpdateInvestmentPriceDto dto);
    Task<bool> DeleteInvestmentAsync(long investmentId, long userId);
    Task<List<InvestmentByAssetTypeDto>> GetPortfolioBreakdownAsync(long userId);
}

public class InvestmentService : IInvestmentService
{
    private readonly ExpenseManagerContext _context;

    public InvestmentService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific investment by ID
    /// </summary>
    public async Task<InvestmentResponseDto?> GetInvestmentByIdAsync(long investmentId, long userId)
    {
        var investment = await _context.Investments
            .Include(i => i.Account)
            .Where(i => i.Id == investmentId && i.UserId == userId)
            .FirstOrDefaultAsync();

        if (investment == null)
            return null;

        return MapToResponseDto(investment);
    }

    /// <summary>
    /// Get all investments for a user
    /// </summary>
    public async Task<List<InvestmentResponseDto>> GetUserInvestmentsAsync(long userId, string? assetType = null)
    {
        var query = _context.Investments
            .Include(i => i.Account)
            .Where(i => i.UserId == userId);

        if (!string.IsNullOrWhiteSpace(assetType))
            query = query.Where(i => i.AssetType == assetType);

        var investments = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return investments.Select(MapToResponseDto).ToList();
    }

    /// <summary>
    /// Get complete portfolio summary with P/L
    /// </summary>
    public async Task<InvestmentPortfolioDto> GetPortfolioSummaryAsync(long userId)
    {
        var investments = await GetUserInvestmentsAsync(userId);
        var breakdown = await GetPortfolioBreakdownAsync(userId);

        var totalInvested = investments.Sum(i => i.TotalInvested);
        var totalCurrentValue = investments.Sum(i => i.TotalCurrentValue ?? 0);
        var totalProfitLoss = totalCurrentValue - totalInvested;
        var totalProfitLossPercentage = totalInvested > 0 ? (totalProfitLoss / totalInvested) * 100 : 0;

        return new InvestmentPortfolioDto
        {
            TotalInvestments = investments.Count,
            TotalInvested = totalInvested,
            TotalCurrentValue = totalCurrentValue,
            TotalProfitLoss = totalProfitLoss,
            TotalProfitLossPercentage = totalProfitLossPercentage,
            IsOverallProfit = totalProfitLoss >= 0,
            ByAssetType = breakdown,
            Investments = investments
        };
    }

    /// <summary>
    /// Create a new investment
    /// </summary>
    public async Task<InvestmentResponseDto> CreateInvestmentAsync(long userId, CreateInvestmentDto dto)
    {
        // Verify account if provided
        if (dto.AccountId.HasValue)
        {
            var account = await _context.Accounts
                .Where(a => a.Id == dto.AccountId.Value && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (account == null)
                throw new InvalidOperationException("Account not found");
        }

        var investment = new Investment
        {
            UserId = userId,
            AccountId = dto.AccountId,
            Name = dto.Name,
            AssetType = dto.AssetType,
            Quantity = dto.Quantity,
            PurchasePrice = dto.PurchasePrice,
            PurchaseDate = dto.PurchaseDate,
            CurrentValue = dto.CurrentValue,
            LastUpdated = dto.CurrentValue.HasValue ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Investments.Add(investment);
        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(investment).Reference(i => i.Account).LoadAsync();

        return MapToResponseDto(investment);
    }

    /// <summary>
    /// Update an existing investment
    /// </summary>
    public async Task<InvestmentResponseDto> UpdateInvestmentAsync(long userId, UpdateInvestmentDto dto)
    {
        var investment = await _context.Investments
            .Include(i => i.Account)
            .Where(i => i.Id == dto.Id && i.UserId == userId)
            .FirstOrDefaultAsync();

        if (investment == null)
            throw new InvalidOperationException("Investment not found");

        // Update fields
        if (!string.IsNullOrWhiteSpace(dto.Name))
            investment.Name = dto.Name;

        if (dto.Quantity.HasValue)
            investment.Quantity = dto.Quantity.Value;

        if (dto.CurrentValue.HasValue)
        {
            investment.CurrentValue = dto.CurrentValue.Value;
            investment.LastUpdated = DateTime.UtcNow;
        }

        investment.UpdatedAt = DateTime.UtcNow;

        _context.Investments.Update(investment);
        await _context.SaveChangesAsync();

        return MapToResponseDto(investment);
    }

    /// <summary>
    /// Update market price for an investment
    /// </summary>
    public async Task<InvestmentResponseDto> UpdateMarketPriceAsync(long userId, UpdateInvestmentPriceDto dto)
    {
        var investment = await _context.Investments
            .Include(i => i.Account)
            .Where(i => i.Id == dto.Id && i.UserId == userId)
            .FirstOrDefaultAsync();

        if (investment == null)
            throw new InvalidOperationException("Investment not found");

        investment.CurrentValue = dto.CurrentValue;
        investment.LastUpdated = DateTime.UtcNow;
        investment.UpdatedAt = DateTime.UtcNow;

        _context.Investments.Update(investment);
        await _context.SaveChangesAsync();

        return MapToResponseDto(investment);
    }

    /// <summary>
    /// Delete an investment
    /// </summary>
    public async Task<bool> DeleteInvestmentAsync(long investmentId, long userId)
    {
        var investment = await _context.Investments
            .Where(i => i.Id == investmentId && i.UserId == userId)
            .FirstOrDefaultAsync();

        if (investment == null)
            return false;

        _context.Investments.Remove(investment);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Get portfolio breakdown by asset type
    /// </summary>
    public async Task<List<InvestmentByAssetTypeDto>> GetPortfolioBreakdownAsync(long userId)
    {
        var investments = await GetUserInvestmentsAsync(userId);

        var totalPortfolioValue = investments.Sum(i => i.TotalCurrentValue ?? i.TotalInvested);

        var breakdown = investments
            .GroupBy(i => i.AssetType)
            .Select(g => new InvestmentByAssetTypeDto
            {
                AssetType = g.Key,
                Count = g.Count(),
                TotalInvested = g.Sum(i => i.TotalInvested),
                TotalCurrentValue = g.Sum(i => i.TotalCurrentValue ?? 0),
                ProfitLoss = g.Sum(i => i.ProfitLoss ?? 0),
                ProfitLossPercentage = g.Sum(i => i.TotalInvested) > 0
                    ? (g.Sum(i => i.ProfitLoss ?? 0) / g.Sum(i => i.TotalInvested)) * 100
                    : 0,
                PortfolioPercentage = totalPortfolioValue > 0
                    ? (g.Sum(i => i.TotalCurrentValue ?? i.TotalInvested) / totalPortfolioValue) * 100
                    : 0
            })
            .OrderByDescending(b => b.TotalCurrentValue)
            .ToList();

        return breakdown;
    }

    // Helper Methods

    private InvestmentResponseDto MapToResponseDto(Investment investment)
    {
        var totalInvested = investment.Quantity * investment.PurchasePrice;
        decimal? currentMarketPrice = null;
        decimal? totalCurrentValue = null;
        decimal? profitLoss = null;
        decimal? profitLossPercentage = null;
        bool isProfit = false;

        if (investment.CurrentValue.HasValue)
        {
            currentMarketPrice = investment.CurrentValue.Value;
            totalCurrentValue = investment.Quantity * currentMarketPrice.Value;
            profitLoss = totalCurrentValue.Value - totalInvested;
            profitLossPercentage = totalInvested > 0 ? (profitLoss.Value / totalInvested) * 100 : 0;
            isProfit = profitLoss.Value >= 0;
        }

        return new InvestmentResponseDto
        {
            Id = investment.Id,
            UserId = investment.UserId,
            AccountId = investment.AccountId,
            AccountName = investment.Account?.Name,
            Name = investment.Name,
            AssetType = investment.AssetType,
            Quantity = investment.Quantity,
            PurchasePrice = investment.PurchasePrice,
            TotalInvested = totalInvested,
            PurchaseDate = investment.PurchaseDate,
            CurrentValue = investment.CurrentValue,
            CurrentMarketPrice = currentMarketPrice,
            TotalCurrentValue = totalCurrentValue,
            ProfitLoss = profitLoss,
            ProfitLossPercentage = profitLossPercentage,
            IsProfit = isProfit,
            LastUpdated = investment.LastUpdated,
            CreatedAt = investment.CreatedAt,
            UpdatedAt = investment.UpdatedAt
        };
    }
}
