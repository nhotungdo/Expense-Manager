using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Hubs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Wallets;

[Authorize]
public class DetailModel : PageModel
{
    private readonly IAccountService _accountService;
    private readonly ISharedAccountService _sharedAccountService;
    private readonly ITransactionService _transactionService;
    private readonly IFriendshipService _friendshipService;
    private readonly IHubContext<WalletHub> _hubContext;

    public DetailModel(
        IAccountService accountService, 
        ISharedAccountService sharedAccountService, 
        ITransactionService transactionService,
        IFriendshipService friendshipService,
        IHubContext<WalletHub> hubContext)
    {
        _accountService = accountService;
        _sharedAccountService = sharedAccountService;
        _transactionService = transactionService;
        _friendshipService = friendshipService;
        _hubContext = hubContext;
    }

    public AccountResponseDto Workspace { get; set; } = null!;
    public List<SharedAccountResponseDto> Members { get; set; } = new();
    public List<FriendshipDto> Friends { get; set; } = new();
    public int CurrentUserPermission { get; set; } // 2=Full(Owner), 1=Add, 0=View
    public long CurrentUserId { get; set; }
    public decimal MonthSpending { get; set; }
    
    // For Invite
    [BindProperty]
    public string InviteEmail { get; set; } = string.Empty;
    
    [BindProperty]
    public int InvitePermission { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(long id)
    {
        CurrentUserId = GetUserId();
        if (id <= 0 || CurrentUserId <= 0) return RedirectToPage("/Wallets/Index");

        // Check access
        var hasAccess = await _sharedAccountService.CanAccessAccountAsync(id, CurrentUserId);
        if (!hasAccess) return Forbid();

        CurrentUserPermission = await _sharedAccountService.GetPermissionLevelAsync(id, CurrentUserId);

        // Get Account Details
        if (CurrentUserPermission == 2) // Owner or Full Access matches Owner check in some logic
        {
             // Try fetching as owner
             try {
                Workspace = await _accountService.GetAccountByIdAsync(id, CurrentUserId);
             } catch {
                // If I am Full Access (2) but NOT owner in DB (UserId != id), standard service might fail.
                // Fallback to shared list lookup
                await FetchSharedDetail(id);
             }
        }
        else
        {
            await FetchSharedDetail(id);
        }
        
        if (Workspace == null) 
        {
            // Last resort or error
             await FetchSharedDetail(id);
             if (Workspace == null) return NotFound();
        }
        
        // Members
        Members = await _sharedAccountService.GetAccountSharingAsync(id, CurrentUserId); 
        
        // Month Spending (for header Stats)
        var contribution = await _transactionService.GetSpendingContributionAsync(id, CurrentUserId, DateTime.Now.Month, DateTime.Now.Year);
        MonthSpending = contribution.Sum(c => c.TotalAmount);

        // Fetch Friends for Invite if Admin
        if (CurrentUserPermission >= 2)
        {
            Friends = await _friendshipService.GetFriendsAsync(CurrentUserId);
        }

        return Page();
    }
    
    private async Task FetchSharedDetail(long id)
    {
        var sharedWallets = await _sharedAccountService.GetSharedAccountsForUserAsync(CurrentUserId);
        var shared = sharedWallets.FirstOrDefault(w => w.AccountId == id);
        if (shared != null)
        {
            Workspace = new AccountResponseDto 
            { 
                Id = shared.AccountId, 
                Name = shared.AccountName, 
                CurrentBalance = shared.CurrentBalance,
                Currency = shared.Currency,
                Color = shared.Color, 
                Icon = shared.Icon   
            };
        }
    }

    public async Task<IActionResult> OnPostInviteAsync(long id)
    {
        CurrentUserId = GetUserId();
        var permission = await _sharedAccountService.GetPermissionLevelAsync(id, CurrentUserId);
        
        // Full Access (2) allows inviting
        if (permission < 2) return Forbid();

        try 
        {
            await _sharedAccountService.InviteMemberAsync(CurrentUserId, id, InviteEmail, InvitePermission);
            TempData["SuccessMessage"] = "Đã gửi lời mời thành công!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage(new { id = id });
    }

    public async Task<IActionResult> OnPostRemoveMemberAsync(long id, long memberId)
    {
        CurrentUserId = GetUserId();
        var permission = await _sharedAccountService.GetPermissionLevelAsync(id, CurrentUserId);
        
        if (permission < 2) return Forbid();
        
        try 
        {
            await _sharedAccountService.RevokeAccessAsync(CurrentUserId, memberId); 
            TempData["SuccessMessage"] = "Đã xóa thành viên.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Không thể xóa thành viên: " + ex.Message;
        }
        
        return RedirectToPage(new { id = id });
    }

    public async Task<IActionResult> OnPostUpdatePermissionAsync(long id, long sharedAccountId, int newPermission)
    {
        CurrentUserId = GetUserId();
        var permission = await _sharedAccountService.GetPermissionLevelAsync(id, CurrentUserId);
        
        if (permission < 2) return Forbid();

        await _sharedAccountService.UpdatePermissionAsync(CurrentUserId, sharedAccountId, newPermission);
        
        TempData["SuccessMessage"] = "Đã cập nhật quyền hạn.";
        return RedirectToPage(new { id = id });
    }
    
    public async Task<IActionResult> OnPostLeaveAsync(long id)
    {
        CurrentUserId = GetUserId();
        
        // Fetch my link
        var sharedWallets = await _sharedAccountService.GetSharedAccountsForUserAsync(CurrentUserId);
        var myLink = sharedWallets.FirstOrDefault(w => w.AccountId == id);
        
        if (myLink != null)
        {
            await _sharedAccountService.LeaveSharedAccountAsync(CurrentUserId, myLink.Id);
            return RedirectToPage("/Wallets/Index");
        }
        
        return RedirectToPage(new { id = id });
    }

    public async Task<JsonResult> OnGetContributionDataAsync(long id, int month, int year)
    {
        CurrentUserId = GetUserId();
        var data = await _transactionService.GetSpendingContributionAsync(id, CurrentUserId, month, year);
        return new JsonResult(data);
    }

    public async Task<JsonResult> OnGetTransactionsAsync(long id, string search = "", string userFilter = "")
    {
        CurrentUserId = GetUserId();
        // Use generic filter if possible or just fetch all recent and filter here? 
        // Service typically has GetTransactionsByAccountIdAsync with limit.
        // Let's use generic TransactionFilterDto if service supports it, 
        // OR standard GetTransactionsByAccountIdAsync(id, userId, limit)
        
        int limit = 100;
        var transactions = await _transactionService.GetTransactionsByAccountIdAsync(id, CurrentUserId, limit);

        // Apply filters
        var query = transactions.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(t => 
                (t.Note != null && t.Note.ToLower().Contains(search)) || 
                (t.CategoryName != null && t.CategoryName.ToLower().Contains(search))
            );
        }

        if (!string.IsNullOrEmpty(userFilter) && long.TryParse(userFilter, out long filterUserId))
        {
            query = query.Where(t => t.UserId == filterUserId);
        }

        return new JsonResult(query.ToList());
    }

    public async Task<JsonResult> OnGetWalletSummaryAsync(long id)
    {
        CurrentUserId = GetUserId();
        var hasAccess = await _sharedAccountService.CanAccessAccountAsync(id, CurrentUserId);
        if (!hasAccess) return new JsonResult(null);

        AccountResponseDto? wallet = null;
        try 
        {
            wallet = await _accountService.GetAccountByIdAsync(id, CurrentUserId);
        }
        catch
        {
            // Ignore if standard service checks ownership strictly
        }

        if (wallet == null)
        {
             var sharedWallets = await _sharedAccountService.GetSharedAccountsForUserAsync(CurrentUserId);
             var shared = sharedWallets.FirstOrDefault(w => w.AccountId == id);
             if (shared != null)
             {
                 return new JsonResult(new {
                     currentBalance = shared.CurrentBalance,
                     currency = shared.Currency,
                     name = shared.AccountName
                 });
             }
             return new JsonResult(null);
        }
        
        return new JsonResult(new {
            currentBalance = wallet.CurrentBalance,
            currency = wallet.Currency,
            name = wallet.Name
        });
    }
    
    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
