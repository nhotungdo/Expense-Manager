using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MoneyTrackerApp.Services;

/// <summary>
/// Service for managing group expenses and split bills
/// Handles group creation, member management, expense splitting, and debt settlement
/// </summary>
public interface IGroupExpenseService
{
    Task<GroupExpenseResponseDto?> GetGroupByIdAsync(long groupId, long userId);
    Task<List<GroupExpenseResponseDto>> GetUserGroupsAsync(long userId);
    Task<GroupExpenseResponseDto> CreateGroupAsync(long userId, CreateGroupExpenseDto dto);
    Task<GroupExpenseResponseDto> UpdateGroupAsync(long userId, UpdateGroupExpenseDto dto);
    Task<bool> DeleteGroupAsync(long groupId, long userId);
    Task<GroupMemberDto> AddMemberAsync(long userId, AddGroupMemberDto dto);
    Task<bool> RemoveMemberAsync(long groupId, long memberId, long userId);
    Task<GroupTransactionResponseDto> CreateGroupTransactionAsync(long userId, CreateGroupTransactionDto dto);
    Task<List<GroupTransactionResponseDto>> GetGroupTransactionsAsync(long groupId, long userId);
    Task<GroupBalanceSummaryDto> GetGroupBalancesAsync(long groupId, long userId);
    Task<List<GroupDebtDto>> CalculateSettlementsAsync(long groupId, long userId);
}

public class GroupExpenseService : IGroupExpenseService
{
    private readonly ExpenseManagerContext _context;
    private readonly ITransactionService _transactionService;

    public GroupExpenseService(ExpenseManagerContext context, ITransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    /// <summary>
    /// Get a specific group by ID
    /// </summary>
    public async Task<GroupExpenseResponseDto?> GetGroupByIdAsync(long groupId, long userId)
    {
        var group = await _context.GroupExpenses
            .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
            .Include(g => g.CreatedByUser)
            .Include(g => g.GroupTransactions)
            .Where(g => g.Id == groupId && g.GroupMembers.Any(gm => gm.UserId == userId))
            .FirstOrDefaultAsync();

        if (group == null)
            return null;

        return MapToResponseDto(group);
    }

    /// <summary>
    /// Get all groups for a user
    /// </summary>
    public async Task<List<GroupExpenseResponseDto>> GetUserGroupsAsync(long userId)
    {
        var groups = await _context.GroupExpenses
            .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
            .Include(g => g.CreatedByUser)
            .Include(g => g.GroupTransactions)
            .Where(g => g.GroupMembers.Any(gm => gm.UserId == userId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return groups.Select(MapToResponseDto).ToList();
    }

    /// <summary>
    /// Create a new group
    /// </summary>
    public async Task<GroupExpenseResponseDto> CreateGroupAsync(long userId, CreateGroupExpenseDto dto)
    {
        var group = new GroupExpense
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedByUserId = userId,
            IsPublic = dto.IsPublic,
            Icon = dto.Icon,
            Color = dto.Color,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.GroupExpenses.Add(group);
        await _context.SaveChangesAsync();

        // Add creator as owner
        var creatorMember = new GroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow
        };
        _context.GroupMembers.Add(creatorMember);

        // Add other members if provided
        if (dto.MemberUserIds != null && dto.MemberUserIds.Any())
        {
            foreach (var memberUserId in dto.MemberUserIds.Where(id => id != userId))
            {
                var member = new GroupMember
                {
                    GroupId = group.Id,
                    UserId = memberUserId,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                };
                _context.GroupMembers.Add(member);
            }
        }

        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(group).Collection(g => g.GroupMembers).LoadAsync();
        await _context.Entry(group).Reference(g => g.CreatedByUser).LoadAsync();

        return MapToResponseDto(group);
    }

    /// <summary>
    /// Update a group
    /// </summary>
    public async Task<GroupExpenseResponseDto> UpdateGroupAsync(long userId, UpdateGroupExpenseDto dto)
    {
        var group = await _context.GroupExpenses
            .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
            .Include(g => g.CreatedByUser)
            .Include(g => g.GroupTransactions)
            .Where(g => g.Id == dto.Id && g.CreatedByUserId == userId)
            .FirstOrDefaultAsync();

        if (group == null)
            throw new InvalidOperationException("Group not found or you don't have permission");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            group.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Description))
            group.Description = dto.Description;

        if (dto.IsPublic.HasValue)
            group.IsPublic = dto.IsPublic.Value;

        if (!string.IsNullOrWhiteSpace(dto.Icon))
            group.Icon = dto.Icon;

        if (!string.IsNullOrWhiteSpace(dto.Color))
            group.Color = dto.Color;

        group.UpdatedAt = DateTime.UtcNow;

        _context.GroupExpenses.Update(group);
        await _context.SaveChangesAsync();

        return MapToResponseDto(group);
    }

    /// <summary>
    /// Delete a group
    /// </summary>
    public async Task<bool> DeleteGroupAsync(long groupId, long userId)
    {
        var group = await _context.GroupExpenses
            .Include(g => g.GroupMembers)
            .Include(g => g.GroupTransactions)
                .ThenInclude(gt => gt.GroupTransactionSplits)
            .Where(g => g.Id == groupId && g.CreatedByUserId == userId)
            .FirstOrDefaultAsync();

        if (group == null)
            return false;

        // Remove all splits
        foreach (var transaction in group.GroupTransactions)
        {
            _context.GroupTransactionSplits.RemoveRange(transaction.GroupTransactionSplits);
        }

        // Remove all transactions
        _context.GroupTransactions.RemoveRange(group.GroupTransactions);

        // Remove all members
        _context.GroupMembers.RemoveRange(group.GroupMembers);

        // Remove group
        _context.GroupExpenses.Remove(group);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Add a member to a group
    /// </summary>
    public async Task<GroupMemberDto> AddMemberAsync(long userId, AddGroupMemberDto dto)
    {
        // Verify user has permission (is owner or admin)
        var userMember = await _context.GroupMembers
            .Where(gm => gm.GroupId == dto.GroupId && gm.UserId == userId)
            .FirstOrDefaultAsync();

        if (userMember == null || (userMember.Role != "Owner" && userMember.Role != "Admin"))
            throw new InvalidOperationException("You don't have permission to add members");

        // Check if user is already a member
        var existingMember = await _context.GroupMembers
            .Where(gm => gm.GroupId == dto.GroupId && gm.UserId == dto.UserId)
            .FirstOrDefaultAsync();

        if (existingMember != null)
            throw new InvalidOperationException("User is already a member of this group");

        var member = new GroupMember
        {
            GroupId = dto.GroupId,
            UserId = dto.UserId,
            Role = dto.Role,
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(member);
        await _context.SaveChangesAsync();

        // Reload with user
        await _context.Entry(member).Reference(gm => gm.User).LoadAsync();

        return new GroupMemberDto
        {
            Id = member.Id,
            GroupId = member.GroupId,
            UserId = member.UserId,
            UserName = member.User.UserName ?? "Unknown",
            UserEmail = member.User.Email,
            Role = member.Role ?? "Member",
            JoinedAt = member.JoinedAt ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Remove a member from a group
    /// </summary>
    public async Task<bool> RemoveMemberAsync(long groupId, long memberId, long userId)
    {
        // Verify user has permission
        var userMember = await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.UserId == userId)
            .FirstOrDefaultAsync();

        if (userMember == null || (userMember.Role != "Owner" && userMember.Role != "Admin"))
            throw new InvalidOperationException("You don't have permission to remove members");

        var member = await _context.GroupMembers
            .Where(gm => gm.Id == memberId && gm.GroupId == groupId)
            .FirstOrDefaultAsync();

        if (member == null)
            return false;

        // Cannot remove owner
        if (member.Role == "Owner")
            throw new InvalidOperationException("Cannot remove group owner");

        _context.GroupMembers.Remove(member);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Create a group transaction with explicit splits
    /// </summary>
    public async Task<GroupTransactionResponseDto> CreateGroupTransactionAsync(long userId, CreateGroupTransactionDto dto)
    {
        // Verify creator is a member
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == dto.GroupId && gm.UserId == userId);

        if (!isMember)
            throw new InvalidOperationException("You are not a member of this group");

        // Verify payer is a member
        var isPayerMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == dto.GroupId && gm.UserId == dto.PaidByUserId);

        if (!isPayerMember)
            throw new InvalidOperationException("Payer is not a member of this group");

        var transaction = new GroupTransaction
        {
            GroupId = dto.GroupId,
            PaidByUserId = dto.PaidByUserId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Description = dto.Description,
            TransactionDate = dto.TransactionDate,
            Category = dto.Category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.GroupTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Handle Personal Wallet Sync
        if (dto.SyncToPersonalWallet && dto.PersonalWalletId.HasValue && dto.PaidByUserId == userId)
        {
            try
            {
                var personalTransaction = new CreateTransactionDto
                {
                    AccountId = dto.PersonalWalletId.Value,
                    Amount = dto.Amount,
                    TransactionType = 2, // Expense
                    Note = $"Group Expense: {dto.Description}",
                    TransactionDate = dto.TransactionDate,
                    // Try to map category by name if exists, or leave null
                    // For now we skip category mapping to avoid complexity, or we could look it up
                };

                await _transactionService.CreateTransactionAsync(userId, personalTransaction);
            }
            catch (Exception)
            {
                // Log warning but don't fail the group transaction? 
                // Or allow it to bubble up? 
                // Requirement doesn't specify. Safe to ignore for now or let user know.
                // We'll let it fail so user knows sync failed.
                throw;
            }
        }

        // Create splits
        var splits = new List<GroupTransactionSplit>();
        foreach (var item in dto.Splits)
        {
            splits.Add(new GroupTransactionSplit
            {
                GroupTransactionId = transaction.Id,
                UserId = item.UserId,
                Amount = item.Amount,
                // If the person who owes is also the one who paid, they have "paid" their share (it's their own expense)
                IsPaid = (item.UserId == dto.PaidByUserId)
            });
        }

        _context.GroupTransactionSplits.AddRange(splits);
        await _context.SaveChangesAsync();

        // Reload with includes
        await _context.Entry(transaction).Reference(t => t.Group).LoadAsync();
        await _context.Entry(transaction).Reference(t => t.PaidByUser).LoadAsync();
        await _context.Entry(transaction).Collection(t => t.GroupTransactionSplits).LoadAsync();

        return MapToTransactionResponseDto(transaction);
    }

    /// <summary>
    /// Get all transactions for a group
    /// </summary>
    public async Task<List<GroupTransactionResponseDto>> GetGroupTransactionsAsync(long groupId, long userId)
    {
        // Verify user is a member
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
            throw new InvalidOperationException("You are not a member of this group");

        var transactions = await _context.GroupTransactions
            .Include(t => t.Group)
            .Include(t => t.PaidByUser)
            .Include(t => t.GroupTransactionSplits)
            .Where(t => t.GroupId == groupId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return transactions.Select(MapToTransactionResponseDto).ToList();
    }

    /// <summary>
    /// Get group balances (who owes whom)
    /// </summary>
    /// <summary>
    /// Get group balances (who owes whom)
    /// </summary>
    public async Task<GroupBalanceSummaryDto> GetGroupBalancesAsync(long groupId, long userId)
    {
        // Verify user is a member
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
            throw new InvalidOperationException("You are not a member of this group");

        var memberBalances = await CalculateMemberBalancesAsync(groupId);
        var settlements = CalculateOptimalSettlements(memberBalances);

        var groupName = await _context.GroupExpenses
            .Where(g => g.Id == groupId)
            .Select(g => g.Name)
            .FirstOrDefaultAsync() ?? "Unknown";

        return new GroupBalanceSummaryDto
        {
            GroupId = groupId,
            GroupName = groupName,
            MemberBalances = memberBalances,
            Settlements = settlements
        };
    }

    /// <summary>
    /// Calculate optimal debt settlements
    /// </summary>
    public async Task<List<GroupDebtDto>> CalculateSettlementsAsync(long groupId, long userId)
    {
        // Verify user is a member
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (!isMember)
            throw new InvalidOperationException("You are not a member of this group");

        var memberBalances = await CalculateMemberBalancesAsync(groupId);
        return CalculateOptimalSettlements(memberBalances);
    }

    // Helper: Calculate raw member balances
    private async Task<List<GroupMemberBalanceDto>> CalculateMemberBalancesAsync(long groupId)
    {
        var group = await _context.GroupExpenses
            .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new InvalidOperationException("Group not found");

        var transactions = await _context.GroupTransactions
            .Include(t => t.GroupTransactionSplits)
            .Where(t => t.GroupId == groupId)
            .ToListAsync();

        var memberBalances = new List<GroupMemberBalanceDto>();

        foreach (var member in group.GroupMembers)
        {
            var totalPaid = transactions
                .Where(t => t.PaidByUserId == member.UserId)
                .Sum(t => t.Amount);

            var totalOwed = transactions
                .SelectMany(t => t.GroupTransactionSplits)
                .Where(s => s.UserId == member.UserId)
                .Sum(s => s.Amount);

            var balance = totalPaid - totalOwed;

            memberBalances.Add(new GroupMemberBalanceDto
            {
                UserId = member.UserId,
                UserName = member.User.UserName ?? "Unknown",
                TotalPaid = totalPaid,
                TotalOwed = totalOwed,
                Balance = balance
            });
        }
        
        return memberBalances;
    }

    // Helper: Pure logic for settlements
    private List<GroupDebtDto> CalculateOptimalSettlements(List<GroupMemberBalanceDto> memberBalances)
    {
        var settlements = new List<GroupDebtDto>();
        
        // Clone to avoid modifying original references if needed, but here we work with DTOs
        // Make sure we don't modify the input list objects if they are used elsewhere (they are used in GetGroupBalancesAsync return)
        // So we should clone the balances for calculation
        var workingBalances = memberBalances.Select(b => new { b.UserId, b.UserName, Balance = b.Balance }).ToList();

        var creditors = workingBalances.Where(b => b.Balance > 0).OrderByDescending(b => b.Balance).ToList();
        var debtors = workingBalances.Where(b => b.Balance < 0).OrderBy(b => b.Balance).ToList();

        int i = 0, j = 0;
        // Use a wrapper class or modify local variable to track remaining balance
        // Since anonymous types are read-only, let's use a small local class or dictionary
        var creditorBalances = creditors.ToDictionary(c => c.UserId, c => c.Balance);
        var debtorBalances = debtors.ToDictionary(d => d.UserId, d => d.Balance);

        // Re-build lists of IDs to iterate
        var creditorIds = creditors.Select(c => c.UserId).ToList();
        var debtorIds = debtors.Select(d => d.UserId).ToList();

        while (i < creditorIds.Count && j < debtorIds.Count)
        {
            var creditorId = creditorIds[i];
            var debtorId = debtorIds[j];
            
            var creditAmount = creditorBalances[creditorId];
            var debtAmount = debtorBalances[debtorId]; // Negative

            var settlementAmount = Math.Min(creditAmount, Math.Abs(debtAmount));

            // Find names
            var creditorName = workingBalances.First(b => b.UserId == creditorId).UserName;
            var debtorName = workingBalances.First(b => b.UserId == debtorId).UserName;

            settlements.Add(new GroupDebtDto
            {
                FromUserId = debtorId,
                FromUserName = debtorName,
                ToUserId = creditorId,
                ToUserName = creditorName,
                Amount = settlementAmount
            });

            creditorBalances[creditorId] -= settlementAmount;
            debtorBalances[debtorId] += settlementAmount;

            // Use a small epsilon for float comparison if needed, but decimal is precise
            if (creditorBalances[creditorId] == 0) i++;
            if (debtorBalances[debtorId] == 0) j++;
        }

        return settlements;
    }

    // Helper Methods

    private GroupExpenseResponseDto MapToResponseDto(GroupExpense group)
    {
        return new GroupExpenseResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            CreatedByUserId = group.CreatedByUserId,
            CreatedByUserName = group.CreatedByUser?.UserName ?? "Unknown",
            IsPublic = group.IsPublic,
            Icon = group.Icon,
            Color = group.Color,
            MemberCount = group.GroupMembers.Count,
            TotalExpenses = group.GroupTransactions.Sum(t => t.Amount),
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            Members = group.GroupMembers.Select(gm => new GroupMemberDto
            {
                Id = gm.Id,
                GroupId = gm.GroupId,
                UserId = gm.UserId,
                UserName = gm.User?.UserName ?? "Unknown",
                UserEmail = gm.User?.Email,
                Role = gm.Role ?? "Member",
                JoinedAt = gm.JoinedAt ?? DateTime.UtcNow
            }).ToList()
        };
    }

    private GroupTransactionResponseDto MapToTransactionResponseDto(GroupTransaction transaction)
    {
        return new GroupTransactionResponseDto
        {
            Id = transaction.Id,
            GroupId = transaction.GroupId,
            GroupName = transaction.Group?.Name ?? "Unknown",
            PaidByUserId = transaction.PaidByUserId,
            PaidByUserName = transaction.PaidByUser?.UserName ?? "Unknown",
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            Category = transaction.Category,
            CreatedAt = transaction.CreatedAt,
            Splits = transaction.GroupTransactionSplits.Select(s => new GroupTransactionSplitDto
            {
                UserId = s.UserId,
                Amount = s.Amount,
                IsPaid = s.IsPaid
            }).ToList()
        };
    }
}
