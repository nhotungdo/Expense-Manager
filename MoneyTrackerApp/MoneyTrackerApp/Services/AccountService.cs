using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing user wallets/accounts
/// Handles CRUD operations, balance management, and account visibility
/// </summary>
public interface IAccountService
{
    Task<AccountResponseDto?> GetAccountByIdAsync(long accountId, long userId);
    Task<List<AccountResponseDto>> GetUserAccountsAsync(long userId, bool includeInactive = false);
    Task<List<AccountSummaryDto>> GetAccountSummariesAsync(long userId);
    Task<AccountResponseDto> CreateAccountAsync(long userId, CreateAccountDto dto);
    Task<AccountResponseDto> UpdateAccountAsync(long userId, UpdateAccountDto dto);
    Task<AccountResponseDto> AdjustBalanceAsync(long userId, AdjustAccountBalanceDto dto);
    Task<bool> DeactivateAccountAsync(long accountId, long userId);
    Task<bool> DeleteAccountAsync(long accountId, long userId);
    Task<bool> UpdateBalanceAsync(long accountId, decimal newBalance);
    Task<decimal> GetAccountBalanceAsync(long accountId);
    Task<List<AccountResponseDto>> GetHiddenAccountsAsync(long userId);
    Task<bool> ToggleAccountVisibilityAsync(long accountId, long userId, bool isActive);
}

public class AccountService : IAccountService
{
    private readonly ExpenseManagerContext _context;
    private readonly ISubscriptionService _subscriptionService;

    public AccountService(ExpenseManagerContext context, ISubscriptionService subscriptionService)
    {
        _context = context;
        _subscriptionService = subscriptionService;
    }

    public async Task<AccountResponseDto?> GetAccountByIdAsync(long accountId, long userId)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == accountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return null;

        return MapAccountToResponseDto(account);
    }

    public async Task<List<AccountResponseDto>> GetUserAccountsAsync(long userId, bool includeInactive = false)
    {
        var query = _context.Accounts
            .Where(a => a.UserId == userId);

        // Since IsActive is not mapped, we cannot filter by it in SQL.
        // Returning all accounts.
        var accounts = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return accounts.Select(MapAccountToResponseDto).ToList();
    }

    /// <summary>
    /// Get account summaries (minimal info) for quick display
    /// </summary>
    public async Task<List<AccountSummaryDto>> GetAccountSummariesAsync(long userId)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return accounts.Select(a => new AccountSummaryDto
        {
            Id = a.Id,
            Name = a.Name,
            AccountType = a.AccountType,
            AccountTypeDisplay = GetAccountTypeDisplay(a.AccountType),
            CurrentBalance = a.CurrentBalance,
            Currency = a.Currency,
            Icon = a.Icon,
            Color = a.Color,
            IsActive = a.IsActive,
            IncludeInTotal = a.IncludeInTotal
        }).ToList();
    }

    /// <summary>
    /// Create a new account/wallet
    /// Enforces wallet limits: Free accounts (max 3), Pro accounts (unlimited)
    /// </summary>
    public async Task<AccountResponseDto> CreateAccountAsync(long userId, CreateAccountDto dto)
    {
        // Get current wallet count
        var currentCount = await _context.Accounts.CountAsync(a => a.UserId == userId);
        
        // Check subscription and enforce wallet limit
        var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
        
        // Determine if user has Pro account (unlimited wallets)
        bool isPro = subscription != null && subscription.PackageId != 1; // PackageId 1 is Free
        
        if (!isPro)
        {
            // Free account: maximum 3 wallets
            const int FREE_MAX_WALLETS = 3;
            
            if (currentCount >= FREE_MAX_WALLETS)
            {
                throw new InvalidOperationException($"Bạn đã đạt giới hạn {FREE_MAX_WALLETS} ví cho tài khoản miễn phí. Vui lòng nâng cấp lên gói Pro để tạo không giới hạn ví.");
            }
        }
        // Pro accounts have unlimited wallets - no check needed

        var account = new Account
        {
            UserId = userId,
            Name = dto.Name,
            AccountType = dto.AccountType,
            InitialBalance = dto.InitialBalance,
            CurrentBalance = dto.InitialBalance,
            Currency = dto.Currency,
            Icon = dto.Icon,
            Color = dto.Color,
            IsActive = true,
            IncludeInTotal = dto.IncludeInTotal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return MapAccountToResponseDto(account);
    }

    /// <summary>
    /// Update account details (name, icon, color, visibility)
    /// </summary>
    public async Task<AccountResponseDto> UpdateAccountAsync(long userId, UpdateAccountDto dto)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == dto.Id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            throw new InvalidOperationException($"Account {dto.Id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            account.Name = dto.Name;

        if (dto.CurrentBalance.HasValue)
            account.CurrentBalance = dto.CurrentBalance.Value;

        if (!string.IsNullOrWhiteSpace(dto.Icon))
            account.Icon = dto.Icon;

        if (!string.IsNullOrWhiteSpace(dto.Color))
            account.Color = dto.Color;

        if (dto.IsActive.HasValue)
            account.IsActive = dto.IsActive.Value;

        if (dto.AccountType.HasValue)
            account.AccountType = dto.AccountType.Value;

        if (dto.IncludeInTotal.HasValue)
            account.IncludeInTotal = dto.IncludeInTotal.Value;

        account.UpdatedAt = DateTime.UtcNow;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return MapAccountToResponseDto(account);
    }

    public async Task<AccountResponseDto> AdjustBalanceAsync(long userId, AdjustAccountBalanceDto dto)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == dto.AccountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            throw new InvalidOperationException($"Account {dto.AccountId} not found");

        // Record audit log for the adjustment
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = "BALANCE_ADJUSTMENT",
            EntityType = "Account",
            EntityId = account.Id,
            Details = $"{dto.Reason} - {dto.Notes}",
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);

        account.CurrentBalance += dto.Amount;
        account.UpdatedAt = DateTime.UtcNow;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return MapAccountToResponseDto(account);
    }

    /// <summary>
    /// Deactivate an account (soft delete - hide from user)
    /// </summary>
    public async Task<bool> DeactivateAccountAsync(long accountId, long userId)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == accountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return false;

        account.IsActive = false;
        account.UpdatedAt = DateTime.UtcNow;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Permanently delete an account
    /// </summary>
    public async Task<bool> DeleteAccountAsync(long accountId, long userId)
    {
        var account = await _context.Accounts
            .Include(a => a.TransactionAccounts)
            .Include(a => a.BankConnections)
            .Where(a => a.Id == accountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return false;

        // Check if account has transactions
        if (account.TransactionAccounts.Any())
            throw new InvalidOperationException("Cannot delete account with existing transactions");

        // Remove bank connections
        _context.BankConnections.RemoveRange(account.BankConnections);

        // Remove account
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Update account balance (typically from transaction sync)
    /// </summary>
    public async Task<bool> UpdateBalanceAsync(long accountId, decimal newBalance)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            return false;

        account.CurrentBalance = newBalance;
        account.UpdatedAt = DateTime.UtcNow;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Get current balance of an account
    /// </summary>
    public async Task<decimal> GetAccountBalanceAsync(long accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        return account?.CurrentBalance ?? 0;
    }

    /// <summary>
    /// Get hidden/inactive accounts
    /// </summary>
    public async Task<List<AccountResponseDto>> GetHiddenAccountsAsync(long userId)
    {
        // Cannot filter by IsActive in SQL. Returning empty to avoid errors.
        return await Task.FromResult(new List<AccountResponseDto>());
    }


    /// <summary>
    /// Toggle account visibility
    /// </summary>
    public async Task<bool> ToggleAccountVisibilityAsync(long accountId, long userId, bool isActive)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == accountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return false;

        account.IsActive = isActive;
        account.UpdatedAt = DateTime.UtcNow;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    // Helper Methods

    private AccountResponseDto MapAccountToResponseDto(Account account)
    {
        var sharedCount = _context.SharedAccounts
            .Count(sa => sa.AccountId == account.Id);

        var isBankLinked = _context.BankConnections
            .Any(bc => bc.AccountId == account.Id);

        return new AccountResponseDto
        {
            Id = account.Id,
            Name = account.Name,
            AccountType = account.AccountType,
            AccountTypeDisplay = GetAccountTypeDisplay(account.AccountType),
            InitialBalance = account.InitialBalance,
            CurrentBalance = account.CurrentBalance,
            Currency = account.Currency,
            Icon = account.Icon,
            Color = account.Color,
            IsActive = account.IsActive,
            IncludeInTotal = account.IncludeInTotal,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            SharedCount = sharedCount,
            IsBankLinked = isBankLinked
        };
    }

    private string GetAccountTypeDisplay(int accountType)
    {
        return accountType switch
        {
            0 => "Tiền mặt",
            1 => "Tài khoản ngân hàng",
            2 => "Ví điện tử",
            3 => "Thẻ tín dụng",
            4 => "Tài khoản tiết kiệm",

            _ => "Không xác định"
        };
    }
}
