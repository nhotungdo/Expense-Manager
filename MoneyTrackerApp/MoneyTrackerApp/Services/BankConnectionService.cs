using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing bank account connections and transaction synchronization
/// Handles Plaid, VietQR, and Open Banking VN API integrations
/// </summary>
public interface IBankConnectionService
{
    Task<BankConnectionResponseDto?> GetBankConnectionAsync(long connectionId, long userId);
    Task<List<BankConnectionResponseDto>> GetUserBankConnectionsAsync(long userId);
    Task<BankConnectionResponseDto> LinkBankAccountAsync(long userId, LinkBankAccountDto dto);
    Task<BankConnectionResponseDto> UpdateBankConnectionAsync(long connectionId, long userId, string accessToken);
    Task<bool> UnlinkBankAccountAsync(long connectionId, long userId);
    Task<bool> UpdateSyncStatusAsync(long connectionId, string status);
    Task<bool> UpdateLastSyncAsync(long connectionId);
    Task<List<BankConnectionResponseDto>> GetExpiredConnectionsAsync();
}

public class BankConnectionService : IBankConnectionService
{
    private readonly ExpenseManagerContext _context;

    public BankConnectionService(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a specific bank connection
    /// </summary>
    public async Task<BankConnectionResponseDto?> GetBankConnectionAsync(long connectionId, long userId)
    {
        var connection = await _context.BankConnections
            .Include(bc => bc.Account)
            .Where(bc => bc.Id == connectionId && bc.UserId == userId)
            .FirstOrDefaultAsync();

        if (connection == null)
            return null;

        return MapToResponseDto(connection);
    }

    /// <summary>
    /// Get all bank connections for a user
    /// </summary>
    public async Task<List<BankConnectionResponseDto>> GetUserBankConnectionsAsync(long userId)
    {
        var connections = await _context.BankConnections
            .Include(bc => bc.Account)
            .Where(bc => bc.UserId == userId)
            .OrderByDescending(bc => bc.CreatedAt)
            .ToListAsync();

        return connections.Select(MapToResponseDto).ToList();
    }

    /// <summary>
    /// Link a bank account to the user's account
    /// </summary>
    public async Task<BankConnectionResponseDto> LinkBankAccountAsync(long userId, LinkBankAccountDto dto)
    {
        // Verify the account belongs to the user
        var account = await _context.Accounts
            .Where(a => a.Id == dto.AccountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            throw new InvalidOperationException("Account not found or you don't have permission to link it");

        // Check if already connected
        var existing = await _context.BankConnections
            .Where(bc => bc.AccountId == dto.AccountId && bc.UserId == userId)
            .FirstOrDefaultAsync();

        if (existing != null)
            throw new InvalidOperationException("This account is already linked to a bank account");

        var connection = new BankConnection
        {
            UserId = userId,
            AccountId = dto.AccountId,
            Provider = dto.Provider,
            AccessToken = dto.AccessToken,
            ItemId = dto.ItemId,
            SyncStatus = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.BankConnections.Add(connection);
        await _context.SaveChangesAsync();

        return MapToResponseDto(connection);
    }

    /// <summary>
    /// Update bank connection access token (refresh token)
    /// </summary>
    public async Task<BankConnectionResponseDto> UpdateBankConnectionAsync(long connectionId, long userId, string accessToken)
    {
        var connection = await _context.BankConnections
            .Include(bc => bc.Account)
            .Where(bc => bc.Id == connectionId && bc.UserId == userId)
            .FirstOrDefaultAsync();

        if (connection == null)
            throw new InvalidOperationException("Bank connection not found");

        connection.AccessToken = accessToken;
        connection.SyncStatus = "Active";

        _context.BankConnections.Update(connection);
        await _context.SaveChangesAsync();

        return MapToResponseDto(connection);
    }

    /// <summary>
    /// Unlink a bank account
    /// </summary>
    public async Task<bool> UnlinkBankAccountAsync(long connectionId, long userId)
    {
        var connection = await _context.BankConnections
            .Where(bc => bc.Id == connectionId && bc.UserId == userId)
            .FirstOrDefaultAsync();

        if (connection == null)
            return false;

        _context.BankConnections.Remove(connection);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Update sync status (Active, Expired, Error)
    /// </summary>
    public async Task<bool> UpdateSyncStatusAsync(long connectionId, string status)
    {
        var connection = await _context.BankConnections.FindAsync(connectionId);
        if (connection == null)
            return false;

        connection.SyncStatus = status;
        _context.BankConnections.Update(connection);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Update last sync timestamp
    /// </summary>
    public async Task<bool> UpdateLastSyncAsync(long connectionId)
    {
        var connection = await _context.BankConnections.FindAsync(connectionId);
        if (connection == null)
            return false;

        connection.LastSync = DateTime.UtcNow;
        _context.BankConnections.Update(connection);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Get all expired/error bank connections
    /// </summary>
    public async Task<List<BankConnectionResponseDto>> GetExpiredConnectionsAsync()
    {
        var connections = await _context.BankConnections
            .Include(bc => bc.Account)
            .Where(bc => bc.SyncStatus == "Expired" || bc.SyncStatus == "Error")
            .OrderByDescending(bc => bc.LastSync)
            .ToListAsync();

        return connections.Select(MapToResponseDto).ToList();
    }

    // Helper Methods

    private BankConnectionResponseDto MapToResponseDto(BankConnection connection)
    {
        return new BankConnectionResponseDto
        {
            Id = connection.Id,
            AccountId = connection.AccountId,
            AccountName = connection.Account?.Name ?? "Unknown",
            Provider = connection.Provider,
            ItemId = connection.ItemId,
            LastSync = connection.LastSync,
            SyncStatus = connection.SyncStatus,
            CreatedAt = connection.CreatedAt
        };
    }
}
