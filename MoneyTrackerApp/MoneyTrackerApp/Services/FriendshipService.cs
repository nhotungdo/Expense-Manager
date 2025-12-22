using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services;

public interface IFriendshipService
{
    Task<List<FriendDto>> SearchUsersAsync(string query, long currentUserId);
    Task<bool> SendFriendRequestAsync(long requesterId, long receiverId);
    Task<bool> AcceptFriendRequestAsync(long friendshipId, long userId);
    Task<bool> RemoveFriendAsync(long friendshipId, long userId);
    Task<List<FriendshipDto>> GetFriendsAsync(long userId);
    Task<List<FriendshipDto>> GetPendingRequestsAsync(long userId); // Received
    Task<List<FriendshipDto>> GetSentRequestsAsync(long userId);
}

public class FriendshipService : IFriendshipService
{
    private readonly ExpenseManagerContext _context;

    public FriendshipService(ExpenseManagerContext context)
    {
        _context = context;
    }

    public async Task<List<FriendDto>> SearchUsersAsync(string query, long currentUserId)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<FriendDto>();

        query = query.Trim();

        // Find users by Exact PhoneNumber or UserName
        // Requirement said: Input UserName or PhoneNumber
        var users = await _context.Users
            .Where(u => (u.UserName == query || u.PhoneNumber == query) && u.Id != currentUserId && u.Enabled)
            .Select(u => new FriendDto
            {
                Id = u.Id,
                FullName = u.FullName ?? u.UserName ?? "Unknown",
                ProfilePictureUrl = u.ProfilePictureUrl,
                UserName = u.UserName
            })
            .ToListAsync();

        return users;
    }

    public async Task<bool> SendFriendRequestAsync(long requesterId, long receiverId)
    {
        if (requesterId == receiverId) return false;

        // Check if user exists
        var receiver = await _context.Users.FindAsync(receiverId);
        if (receiver == null || !receiver.Enabled) return false;

        // Check if relationship exists
        var existing = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                (f.RequesterId == requesterId && f.ReceiverId == receiverId) ||
                (f.RequesterId == receiverId && f.ReceiverId == requesterId));

        if (existing != null)
        {
            // If declined/blocked (Status 2), or already friends (1), or pending (0)
            // For now, if connection exists, we don't send new one unless we handle re-friending logic.
            // Requirement doesn't specify deeply about re-friending after block/decline.
            // But if pending, return false.
            return false;
        }

        var friendship = new Friendship
        {
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = 0, // Pending
            CreatedAt = DateTime.UtcNow
        };

        _context.Friendships.Add(friendship);

        // Add Notification
        var requester = await _context.Users.FindAsync(requesterId);
        var notification = new Notification
        {
            UserId = receiverId,
            Title = "Lời mời kết bạn mới",
            Message = $"{requester?.FullName ?? "Ai đó"} muốn kết bạn với bạn.",
            Type = "FriendRequest",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcceptFriendRequestAsync(long friendshipId, long userId)
    {
        // User must be the receiver
        var friendship = await _context.Friendships.FindAsync(friendshipId);
        if (friendship == null) return false;

        if (friendship.ReceiverId != userId) return false;

        if (friendship.Status != 0) return false; // Only accept pending

        friendship.Status = 1; // Accepted
        friendship.UpdatedAt = DateTime.UtcNow;

        // Notify requester
        var receiver = await _context.Users.FindAsync(userId);
        var notif = new Notification
        {
            UserId = friendship.RequesterId,
            Title = "Lời mời kết bạn được chấp nhận",
            Message = $"{receiver?.FullName ?? "Ai đó"} đã chấp nhận lời mời kết bạn của bạn.",
            Type = "FriendAccepted",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(notif);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFriendAsync(long friendshipId, long userId)
    {
        var friendship = await _context.Friendships.FindAsync(friendshipId);
        if (friendship == null) return false;

        // Must be involved
        if (friendship.RequesterId != userId && friendship.ReceiverId != userId) return false;

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<FriendshipDto>> GetFriendsAsync(long userId)
    {
        var friends = await _context.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Receiver)
            .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == 1)
            .ToListAsync();

        return friends.Select(f => {
            var isRequester = f.RequesterId == userId;
            var friend = isRequester ? f.Receiver : f.Requester;
            return new FriendshipDto
            {
                Id = f.Id,
                FriendId = friend.Id,
                FriendName = friend.FullName ?? friend.UserName ?? "Unknown",
                FriendAvatar = friend.ProfilePictureUrl,
                Status = f.Status,
                IsRequester = isRequester,
                CreatedAt = f.CreatedAt
            };
        }).ToList();
    }

    public async Task<List<FriendshipDto>> GetPendingRequestsAsync(long userId)
    {
        // Requests received by user
        var requests = await _context.Friendships
            .Include(f => f.Requester)
            .Where(f => f.ReceiverId == userId && f.Status == 0)
            .ToListAsync();

         return requests.Select(f => new FriendshipDto
            {
                Id = f.Id,
                FriendId = f.Requester.Id,
                FriendName = f.Requester.FullName ?? f.Requester.UserName ?? "Unknown",
                FriendAvatar = f.Requester.ProfilePictureUrl,
                Status = f.Status,
                IsRequester = false,
                CreatedAt = f.CreatedAt
            }).ToList();
    }

    public async Task<List<FriendshipDto>> GetSentRequestsAsync(long userId)
    {
        // Requests sent by user
        var requests = await _context.Friendships
            .Include(f => f.Receiver)
            .Where(f => f.RequesterId == userId && f.Status == 0)
            .ToListAsync();

         return requests.Select(f => new FriendshipDto
            {
                Id = f.Id,
                FriendId = f.Receiver.Id,
                FriendName = f.Receiver.FullName ?? f.Receiver.UserName ?? "Unknown",
                FriendAvatar = f.Receiver.ProfilePictureUrl,
                Status = f.Status,
                IsRequester = true,
                CreatedAt = f.CreatedAt
            }).ToList();
    }
}
