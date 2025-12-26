using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services;

public class ChatService : IChatService
{
    private readonly ExpenseManagerContext _context;
    private readonly ILogger<ChatService> _logger;

    public ChatService(ExpenseManagerContext context, ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MessageDto>> GetChatHistoryAsync(long currentUserId, long otherUserId)
    {
        try
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    IsRead = m.IsRead,
                    AttachmentUrl = m.AttachmentUrl,
                    AttachmentType = m.AttachmentType,
                    AttachmentName = m.AttachmentName,
                    AttachmentSize = m.AttachmentSize,
                    ThumbnailUrl = m.ThumbnailUrl
                })
                .ToListAsync();

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history between {CurrentUserId} and {OtherUserId}", 
                currentUserId, otherUserId);
            throw;
        }
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(long currentUserId)
    {
        try
        {
            var conversations = await _context.Messages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault(),
                    UnreadCount = g.Count(m => m.ReceiverId == currentUserId && !m.IsRead)
                })
                .ToListAsync();

            var conversationDtos = new List<ConversationDto>();
            foreach (var conv in conversations)
            {
                var user = await _context.Users
                    .Where(u => u.Id == conv.OtherUserId)
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.UserName,
                        u.ProfilePictureUrl
                    })
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    conversationDtos.Add(new ConversationDto
                    {
                        UserId = user.Id,
                        FullName = user.FullName ?? user.UserName ?? "Unknown",
                        Avatar = user.ProfilePictureUrl,
                        LastMessageContent = conv.LastMessage?.Content,
                        LastMessageTime = conv.LastMessage?.Timestamp,
                        UnreadCount = conv.UnreadCount,
                        IsOnline = false
                    });
                }
            }

            return conversationDtos.OrderByDescending(c => c.LastMessageTime).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for user {UserId}", currentUserId);
            throw;
        }
    }

    public async Task<MessageDto> SendMessageAsync(long senderId, long receiverId, string content)
    {
        try
        {
            if (senderId == receiverId)
            {
                throw new InvalidOperationException("Cannot send message to yourself");
            }

            var senderExists = await _context.Users.AnyAsync(u => u.Id == senderId);
            if (!senderExists)
            {
                 throw new InvalidOperationException("Sender does not exist");
            }

            var receiverExists = await _context.Users.AnyAsync(u => u.Id == receiverId);
            if (!receiverExists)
            {
                 throw new InvalidOperationException("Receiver does not exist");
            }

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsRead = message.IsRead
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {ReceiverId}", 
                senderId, receiverId);
            throw;
        }
    }

    public async Task<MessageDto> SendMessageWithAttachmentAsync(
        long senderId, 
        long receiverId, 
        string content,
        string attachmentUrl,
        string attachmentType,
        string attachmentName,
        long attachmentSize,
        string? thumbnailUrl = null)
    {
        try
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = string.IsNullOrWhiteSpace(content) ? attachmentName : content,
                Timestamp = DateTime.UtcNow,
                IsRead = false,
                AttachmentUrl = attachmentUrl,
                AttachmentType = attachmentType,
                AttachmentName = attachmentName,
                AttachmentSize = attachmentSize,
                ThumbnailUrl = thumbnailUrl
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsRead = message.IsRead,
                AttachmentUrl = message.AttachmentUrl,
                AttachmentType = message.AttachmentType,
                AttachmentName = message.AttachmentName,
                AttachmentSize = message.AttachmentSize,
                ThumbnailUrl = message.ThumbnailUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message with attachment from {SenderId} to {ReceiverId}", 
                senderId, receiverId);
            throw;
        }
    }

    public async Task MarkMessagesAsReadAsync(long currentUserId, long otherUserId)
    {
        try
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == otherUserId && 
                           m.ReceiverId == currentUserId && 
                           !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Marked {Count} messages as read for user {UserId}", 
                    unreadMessages.Count, currentUserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read for user {UserId}", currentUserId);
            throw;
        }
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        try
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<int> GetUnreadCountFromUserAsync(long currentUserId, long otherUserId)
    {
        try
        {
            return await _context.Messages
                .Where(m => m.SenderId == otherUserId && 
                           m.ReceiverId == currentUserId && 
                           !m.IsRead)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count from user {OtherUserId} for {CurrentUserId}", 
                otherUserId, currentUserId);
            return 0;
        }
    }

    public async Task<bool> DeleteMessageAsync(long messageId, long userId)
    {
        try
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId);

            if (message == null)
            {
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} deleted by user {UserId}", messageId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return false;
        }
    }

    public async Task<List<MessageDto>> SearchMessagesAsync(long userId, string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<MessageDto>();
            }

            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId || m.ReceiverId == userId) &&
                           m.Content.Contains(query))
                .OrderByDescending(m => m.Timestamp)
                .Take(50)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    IsRead = m.IsRead,
                    AttachmentUrl = m.AttachmentUrl,
                    AttachmentType = m.AttachmentType,
                    AttachmentName = m.AttachmentName,
                    AttachmentSize = m.AttachmentSize,
                    ThumbnailUrl = m.ThumbnailUrl
                })
                .ToListAsync();

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages for user {UserId} with query {Query}", 
                userId, query);
            return new List<MessageDto>();
        }
    }
}
