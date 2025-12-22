using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Contacts;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IFriendshipService _friendshipService;

    public IndexModel(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    public List<FriendshipDto> Friends { get; set; } = new();
    public List<FriendshipDto> ReceivedRequests { get; set; } = new();
    public List<FriendshipDto> SentRequests { get; set; } = new();
    public long CurrentUserId { get; set; }

    public async Task OnGetAsync()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        CurrentUserId = userId;
        Friends = await _friendshipService.GetFriendsAsync(userId);
        ReceivedRequests = await _friendshipService.GetPendingRequestsAsync(userId);
        SentRequests = await _friendshipService.GetSentRequestsAsync(userId);
    }

    public async Task<IActionResult> OnGetSearchAsync(string query)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var results = await _friendshipService.SearchUsersAsync(query, userId);
        
        // We might want to mark status for each result (Friend, Pending, None) but simplified for now.
        // Doing a quick client-side check or separate logic is acceptable.
        
        return new JsonResult(results);
    }

    public async Task<IActionResult> OnPostSendRequestAsync(long receiverId)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendshipService.SendFriendRequestAsync(userId, receiverId);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostAcceptRequestAsync(long friendshipId)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendshipService.AcceptFriendRequestAsync(friendshipId, userId);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnPostRemoveFriendAsync(long friendshipId)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendshipService.RemoveFriendAsync(friendshipId, userId);
        return new JsonResult(new { success });
    }
}
