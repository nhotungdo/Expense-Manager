using MoneyTrackerApp.DTOs;

namespace MoneyTrackerApp.Services;

public interface IChatService
{
    Task<List<MessageDto>> GetChatHistoryAsync(long currentUserId, long otherUserId);
    Task<List<ConversationDto>> GetConversationsAsync(long currentUserId);
    Task<MessageDto> SendMessageAsync(long senderId, long receiverId, string content);
    Task<MessageDto> SendMessageWithAttachmentAsync(
        long senderId, 
        long receiverId, 
        string content,
        string attachmentUrl,
        string attachmentType,
        string attachmentName,
        long attachmentSize,
        string? thumbnailUrl = null);
    Task MarkMessagesAsReadAsync(long currentUserId, long otherUserId);
    Task<int> GetUnreadCountAsync(long userId);
    Task<int> GetUnreadCountFromUserAsync(long currentUserId, long otherUserId);
    Task<bool> DeleteMessageAsync(long messageId, long userId);
    Task<List<MessageDto>> SearchMessagesAsync(long userId, string query);
}
