using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendshipController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendshipController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet("friends")]
    public async Task<ActionResult<List<FriendshipDto>>> GetFriends()
    {
        var userId = GetUserId();
        var friends = await _friendshipService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    [HttpGet("requests/received")]
    public async Task<ActionResult<List<FriendshipDto>>> GetReceivedRequests()
    {
        var userId = GetUserId();
        var requests = await _friendshipService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("requests/sent")]
    public async Task<ActionResult<List<FriendshipDto>>> GetSentRequests()
    {
        var userId = GetUserId();
        var requests = await _friendshipService.GetSentRequestsAsync(userId);
        return Ok(requests);
    }
}
