using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MoneyTrackerApp.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdStr))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userIdStr}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdStr))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User-{userIdStr}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
