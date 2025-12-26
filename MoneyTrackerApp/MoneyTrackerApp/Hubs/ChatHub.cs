using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using MoneyTrackerApp.Services;
using System.Collections.Concurrent;

namespace MoneyTrackerApp.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;
    private static readonly ConcurrentDictionary<long, HashSet<string>> OnlineUsers = new();

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task SendMessage(string receiverIdStr, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (long.TryParse(receiverIdStr, out long receiverId))
            {
                var senderIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(senderIdStr, out long senderId))
                {
                    // Save message to database
                    var savedMessage = await _chatService.SendMessageAsync(senderId, receiverId, message);

                    // Send to receiver with full message data
                    await Clients.User(receiverIdStr).SendAsync("ReceiveMessage", 
                        senderId.ToString(), 
                        message, 
                        savedMessage.Timestamp,
                        savedMessage.Id,
                        savedMessage);

                    // Send back to sender for confirmation with full message data
                    await Clients.User(senderIdStr).SendAsync("ReceiveMessage", 
                        senderId.ToString(), 
                        message, 
                        savedMessage.Timestamp,
                        savedMessage.Id,
                        savedMessage);

                    _logger.LogInformation("Message sent from {SenderId} to {ReceiverId}", senderId, receiverId);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error in SendMessage");
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage");
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    public async Task ReadMessage(string otherUserIdStr)
    {
        try
        {
            if (long.TryParse(otherUserIdStr, out long otherUserId))
            {
                var currentUserIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(currentUserIdStr, out long currentUserId))
                {
                    // Mark messages as read in database
                    await _chatService.MarkMessagesAsReadAsync(currentUserId, otherUserId);

                    // Notify the other user that their messages have been read
                    await Clients.User(otherUserIdStr).SendAsync("MessagesRead", currentUserIdStr);

                    _logger.LogInformation("User {CurrentUserId} marked messages from {OtherUserId} as read", 
                        currentUserId, otherUserId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReadMessage");
        }
    }

    public async Task TypingIndicator(string receiverIdStr, bool isTyping)
    {
        try
        {
            var senderIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(senderIdStr))
            {
                await Clients.User(receiverIdStr).SendAsync("UserTyping", senderIdStr, isTyping);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TypingIndicator");
        }
    }

    public async Task<bool> IsUserOnline(string userIdStr)
    {
        if (long.TryParse(userIdStr, out long userId))
        {
            return OnlineUsers.ContainsKey(userId);
        }
        return false;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out long userId))
            {
                // Add user to online users
                OnlineUsers.AddOrUpdate(userId,
                    new HashSet<string> { Context.ConnectionId },
                    (key, existingSet) =>
                    {
                        existingSet.Add(Context.ConnectionId);
                        return existingSet;
                    });

                // Broadcast online status
                await Clients.All.SendAsync("UserStatusChange", userId, true);

                _logger.LogInformation("User {UserId} connected to chat", userId);
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out long userId))
            {
                bool isOffline = false;

                if (OnlineUsers.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);

                    if (connections.Count == 0)
                    {
                        OnlineUsers.TryRemove(userId, out _);
                        isOffline = true;
                    }
                }

                if (isOffline)
                {
                    await Clients.All.SendAsync("UserStatusChange", userId, false);
                    _logger.LogInformation("User {UserId} went offline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync");
        }
    }
}
