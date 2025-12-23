using Microsoft.AspNetCore.SignalR;

namespace MoneyTrackerApp.Hubs;

public class WalletHub : Hub
{
    public async Task JoinWalletGroup(string walletId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Wallet-{walletId}");
    }

    public async Task LeaveWalletGroup(string walletId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Wallet-{walletId}");
    }
    
    // Call this from client after update, or use IHubContext in Controller/Service
    public async Task NotifyWalletUpdate(string walletId)
    {
        await Clients.Group($"Wallet-{walletId}").SendAsync("ReceiveWalletUpdate", walletId);
    }
}
