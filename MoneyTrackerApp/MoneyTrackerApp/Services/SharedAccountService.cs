using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing shared wallets with role-based permissions
/// Handles sharing, permission management, and access control
/// </summary>
public interface ISharedAccountService
{
    Task<SharedAccountResponseDto?> GetSharedAccountAsync(long sharedAccountId, long userId);
    Task<List<SharedAccountListDto>> GetSharedAccountsForUserAsync(long userId);
    Task<List<SharedAccountResponseDto>> GetAccountSharingAsync(long accountId, long userId);
    Task<SharedAccountResponseDto> ShareAccountAsync(long userId, ShareAccountDto dto);
    Task<SharedAccountResponseDto> UpdatePermissionAsync(long userId, long sharedAccountId, int newPermission);
    Task<bool> RevokeAccessAsync(long userId, long sharedAccountId);
    Task<bool> CanAccessAccountAsync(long accountId, long userId);
    Task<int> GetPermissionLevelAsync(long accountId, long userId);
    Task<bool> LeaveSharedAccountAsync(long userId, long sharedAccountId);
    Task<SharedAccountResponseDto> InviteMemberAsync(long senderId, long accountId, string emailOrPhone, int permission);
}

public class SharedAccountService : ISharedAccountService
{
    private readonly ExpenseManagerContext _context;

    public SharedAccountService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific shared account record
    /// </summary>
    public async Task<SharedAccountResponseDto?> GetSharedAccountAsync(long sharedAccountId, long userId)
    {
        var sharedAccount = await _context.SharedAccounts
            .Include(sa => sa.Account)
            .Include(sa => sa.User)
            .Include(sa => sa.SharedByUser)
            .Where(sa => sa.Id == sharedAccountId && sa.SharedByUserId == userId)
            .FirstOrDefaultAsync();

        if (sharedAccount == null)
            return null;

        return MapToSharedAccountResponseDto(sharedAccount);
    }

    /// <summary>
    /// Get all accounts shared with the user
    /// </summary>
    public async Task<List<SharedAccountListDto>> GetSharedAccountsForUserAsync(long userId)
    {
        var sharedAccounts = await _context.SharedAccounts
            .Include(sa => sa.Account)
            .Include(sa => sa.SharedByUser)
            .Where(sa => sa.UserId == userId)
            .OrderByDescending(sa => sa.CreatedAt)
            .ToListAsync();

        return sharedAccounts.Select(MapToSharedAccountListDto).ToList();
    }

    /// <summary>
    /// Get all users this account is shared with
    /// </summary>
    public async Task<List<SharedAccountResponseDto>> GetAccountSharingAsync(long accountId, long userId)
    {
        // Check if user has access (Owner or Shared)
        if (!await CanAccessAccountAsync(accountId, userId))
             return new List<SharedAccountResponseDto>();

        var sharedAccounts = await _context.SharedAccounts
            .Include(sa => sa.Account)
            .Include(sa => sa.User)
            .Include(sa => sa.SharedByUser)
            .Where(sa => sa.AccountId == accountId)
            .OrderByDescending(sa => sa.CreatedAt)
            .ToListAsync();

        return sharedAccounts.Select(MapToSharedAccountResponseDto).ToList();
    }

    /// <summary>
    /// Share an account with another user
    /// </summary>
    public async Task<SharedAccountResponseDto> ShareAccountAsync(long userId, ShareAccountDto dto)
    {
        // Verify the account belongs to the sharing user
        var account = await _context.Accounts
            .Where(a => a.Id == dto.AccountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            throw new InvalidOperationException("Account not found or you don't have permission to share it");

        // Check if already shared with this user
        var existingShare = await _context.SharedAccounts
            .Where(sa => sa.AccountId == dto.AccountId && sa.UserId == dto.UserId)
            .FirstOrDefaultAsync();

        if (existingShare != null)
            throw new InvalidOperationException("This account is already shared with this user");

        // Verify target user exists
        var targetUser = await _context.Users.FindAsync(dto.UserId);
        if (targetUser == null)
            throw new InvalidOperationException("Target user not found");

        var sharedAccount = new SharedAccount
        {
            AccountId = dto.AccountId,
            UserId = dto.UserId,
            Permission = dto.Permission,
            SharedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.SharedAccounts.Add(sharedAccount);
        await _context.SaveChangesAsync();

        // Reload to get full details
        await _context.Entry(sharedAccount).Reference(sa => sa.Account).LoadAsync();
        await _context.Entry(sharedAccount).Reference(sa => sa.User).LoadAsync();
        await _context.Entry(sharedAccount).Reference(sa => sa.SharedByUser).LoadAsync();

        return MapToSharedAccountResponseDto(sharedAccount);
    }

    /// <summary>
    /// Update permission level for a shared account
    /// </summary>
    public async Task<SharedAccountResponseDto> UpdatePermissionAsync(long userId, long sharedAccountId, int newPermission)
    {
        var sharedAccount = await _context.SharedAccounts
            .Include(sa => sa.Account)
            .Include(sa => sa.User)
            .Include(sa => sa.SharedByUser)
            .Where(sa => sa.Id == sharedAccountId && sa.SharedByUserId == userId)
            .FirstOrDefaultAsync();

        if (sharedAccount == null)
            throw new InvalidOperationException("Shared account not found");

        if (newPermission < 0 || newPermission > 2)
            throw new InvalidOperationException("Invalid permission level");

        sharedAccount.Permission = newPermission;

        _context.SharedAccounts.Update(sharedAccount);
        await _context.SaveChangesAsync();

        return MapToSharedAccountResponseDto(sharedAccount);
    }

    /// <summary>
    /// Revoke access to a shared account
    /// </summary>
    public async Task<bool> RevokeAccessAsync(long userId, long sharedAccountId)
    {
        var sharedAccount = await _context.SharedAccounts
            .Where(sa => sa.Id == sharedAccountId && sa.SharedByUserId == userId)
            .FirstOrDefaultAsync();

        if (sharedAccount == null)
            return false;

        _context.SharedAccounts.Remove(sharedAccount);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Check if user has access to an account (owner or shared with permission)
    /// </summary>
    public async Task<bool> CanAccessAccountAsync(long accountId, long userId)
    {
        // Check if user is the owner
        var isOwner = await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.UserId == userId);

        if (isOwner)
            return true;

        // Check if account is shared with user
        var isShared = await _context.SharedAccounts
            .AnyAsync(sa => sa.AccountId == accountId && sa.UserId == userId);

        return isShared;
    }

    /// <summary>
    /// Get permission level for a shared account
    /// 0 = View, 1 = Add, 2 = Full Access
    /// Returns 2 for owner, null if no access
    /// </summary>
    public async Task<int> GetPermissionLevelAsync(long accountId, long userId)
    {
        // Check if user is the owner
        var isOwner = await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.UserId == userId);

        if (isOwner)
            return 2; // Full access for owner

        // Check shared permission
        var sharedAccount = await _context.SharedAccounts
            .Where(sa => sa.AccountId == accountId && sa.UserId == userId)
            .Select(sa => sa.Permission)
            .FirstOrDefaultAsync();

        return sharedAccount;
    }

    /// <summary>
    /// Allow a user to leave a shared account
    /// </summary>
    public async Task<bool> LeaveSharedAccountAsync(long userId, long sharedAccountId)
    {
        var sharedAccount = await _context.SharedAccounts
            .Where(sa => sa.Id == sharedAccountId && sa.UserId == userId)
            .FirstOrDefaultAsync();

        if (sharedAccount == null)
            return false;

        _context.SharedAccounts.Remove(sharedAccount);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Invite a member to the shared account by Email or Phone
    /// </summary>
    public async Task<SharedAccountResponseDto> InviteMemberAsync(long senderId, long accountId, string emailOrPhone, int permission)
    {
        // Find user by Email or Phone
        var targetUser = await _context.Users
            .Where(u => u.Email == emailOrPhone || u.PhoneNumber == emailOrPhone || u.UserName == emailOrPhone)
            .FirstOrDefaultAsync();

        if (targetUser == null)
            throw new InvalidOperationException("User not found with matching Email, Phone, or Username.");

        if (targetUser.Id == senderId)
             throw new InvalidOperationException("You cannot invite yourself.");

        var dto = new ShareAccountDto
        {
            AccountId = accountId,
            UserId = targetUser.Id,
            Permission = permission
        };

        var result = await ShareAccountAsync(senderId, dto);

        // Send Notification
        var sender = await _context.Users.FindAsync(senderId);
        var account = await _context.Accounts.FindAsync(accountId);
        
        var notification = new Notification
        {
            UserId = targetUser.Id,
            Title = "Lời mời tham gia ví chung",
            Message = $"{sender?.FullName ?? "Someone"} đã mời bạn tham gia ví '{account?.Name}' với quyền {result.PermissionDisplay}.",
            Type = "WalletInvite",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            ActionUrl = $"/Wallets/Detail?id={accountId}" 
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return result;
    }

    // Helper Methods

    private SharedAccountResponseDto MapToSharedAccountResponseDto(SharedAccount sharedAccount)
    {
        return new SharedAccountResponseDto
        {
            Id = sharedAccount.Id,
            AccountId = sharedAccount.AccountId,
            AccountName = sharedAccount.Account?.Name ?? "Unknown",
            UserId = sharedAccount.UserId,
            UserName = sharedAccount.User?.UserName ?? "Unknown",
            UserEmail = sharedAccount.User?.Email ?? "Unknown",
            Permission = sharedAccount.Permission,
            PermissionDisplay = GetPermissionDisplay(sharedAccount.Permission),
            SharedByUserId = sharedAccount.SharedByUserId,
            SharedByUserName = sharedAccount.SharedByUser?.UserName ?? "Unknown",
            AvatarUrl = sharedAccount.User?.ProfilePictureUrl,
            CreatedAt = sharedAccount.CreatedAt
        };
    }

    private SharedAccountListDto MapToSharedAccountListDto(SharedAccount sharedAccount)
    {
        return new SharedAccountListDto
        {
            Id = sharedAccount.Id,
            AccountId = sharedAccount.AccountId,
            AccountName = sharedAccount.Account?.Name ?? "Unknown",
            CurrentBalance = sharedAccount.Account?.CurrentBalance ?? 0,
            Currency = sharedAccount.Account?.Currency ?? "USD",
            Permission = sharedAccount.Permission,
            PermissionDisplay = GetPermissionDisplay(sharedAccount.Permission),
            SharedByUserName = sharedAccount.SharedByUser?.UserName ?? "Unknown",
            Color = sharedAccount.Account?.Color,
            Icon = sharedAccount.Account?.Icon
        };
    }

    private string GetPermissionDisplay(int permission)
    {
        return permission switch
        {
            0 => "View",
            1 => "Add",
            2 => "Full Access",
            _ => "Unknown"
        };
    }
}
