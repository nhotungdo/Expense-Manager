using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for calculating net worth and financial summaries
/// Handles asset totaling, debt calculation, and timeline analysis
/// </summary>
public interface INetWorthService
{
    Task<NetWorthDto> CalculateNetWorthAsync(long userId, bool includeHidden = false);
    Task<decimal> GetTotalAssetsAsync(long userId);
    Task<decimal> GetTotalDebtAsync(long userId);
    Task<Dictionary<string, decimal>> GetNetWorthByCurrencyAsync(long userId);
    Task<List<NetWorthByTypeDto>> GetNetWorthByTypeAsync(long userId);
}

public class NetWorthService : INetWorthService
{
    private readonly ExpenseManagerContext _context;

    public NetWorthService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calculate complete net worth summary including assets, debt, and breakdowns
    /// </summary>
    public async Task<NetWorthDto> CalculateNetWorthAsync(long userId, bool includeHidden = false)
    {
        var query = _context.Accounts
            .Where(a => a.UserId == userId && a.IncludeInTotal);

        if (!includeHidden)
        {
            query = query.Where(a => a.IsActive);
        }

        var accounts = await query.ToListAsync();

        var totalAssets = 0m;
        var totalDebt = 0m;

        foreach (var account in accounts)
        {
            if (account.CurrentBalance >= 0)
            {
                totalAssets += account.CurrentBalance;
            }
            else
            {
                totalDebt += Math.Abs(account.CurrentBalance);
            }
        }

        var netWorth = totalAssets - totalDebt;

        var byType = await GetNetWorthByTypeAsync(userId);
        var byCurrency = await GetNetWorthByCurrencyAsync(userId);

        var dto = new NetWorthDto
        {
            TotalAssets = totalAssets,
            TotalDebt = totalDebt,
            NetWorth = netWorth,
            ByAccountType = byType,
            ByCurrency = byCurrency.Select(kvp => new NetWorthByCurrencyDto
            {
                Currency = kvp.Key,
                Balance = kvp.Value,
                Count = accounts.Count(a => a.Currency == kvp.Key)
            }).ToList(),
            UpdatedAt = DateTime.UtcNow
        };

        return dto;
    }

    /// <summary>
    /// Get total assets (positive account balances)
    /// </summary>
    public async Task<decimal> GetTotalAssetsAsync(long userId)
    {
        var totalAssets = await _context.Accounts
            .Where(a => a.UserId == userId
                && a.IncludeInTotal
                && a.IsActive
                && a.CurrentBalance >= 0)
            .SumAsync(a => a.CurrentBalance);

        return totalAssets;
    }

    /// <summary>
    /// Get total debt (absolute value of negative balances, typically from credit cards)
    /// </summary>
    public async Task<decimal> GetTotalDebtAsync(long userId)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId
                && a.IncludeInTotal
                && a.IsActive
                && a.CurrentBalance < 0)
            .ToListAsync();

        return accounts.Sum(a => Math.Abs(a.CurrentBalance));
    }

    /// <summary>
    /// Get net worth broken down by currency
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetNetWorthByCurrencyAsync(long userId)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId
                && a.IncludeInTotal
                && a.IsActive)
            .ToListAsync();

        return accounts
            .GroupBy(a => a.Currency)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.CurrentBalance));
    }

    /// <summary>
    /// Get net worth broken down by account type
    /// </summary>
    public async Task<List<NetWorthByTypeDto>> GetNetWorthByTypeAsync(long userId)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId
                && a.IncludeInTotal
                && a.IsActive)
            .ToListAsync();

        var groupedByType = accounts
            .GroupBy(a => a.AccountType)
            .Select(g => new NetWorthByTypeDto
            {
                AccountType = g.Key,
                AccountTypeDisplay = GetAccountTypeDisplay(g.Key),
                Balance = g.Sum(a => a.CurrentBalance),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Balance)
            .ToList();

        return groupedByType;
    }

    // Helper Methods

    private string GetAccountTypeDisplay(int accountType)
    {
        return accountType switch
        {
            0 => "Cash",
            1 => "Bank Account",
            2 => "eWallet",
            3 => "Credit Card",
            4 => "Savings Account",
            5 => "Investment",
            _ => "Unknown"
        };
    }
}
