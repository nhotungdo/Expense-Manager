namespace MoneyTrackerApp.DTOs;

public class MessageDto
{
    public long Id { get; set; }
    public long SenderId { get; set; }
    public long ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public string? AttachmentName { get; set; }
    public long? AttachmentSize { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<MessageAttachmentDto>? Attachments { get; set; }
}

public class MessageAttachmentDto
{
    public long Id { get; set; }
    public string AttachmentUrl { get; set; } = string.Empty;
    public string AttachmentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ConversationDto
{
    public long UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
}

public class SendMessageRequest
{
    public long ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UploadFileRequest
{
    public long ReceiverId { get; set; }
    public string? Message { get; set; }
}
