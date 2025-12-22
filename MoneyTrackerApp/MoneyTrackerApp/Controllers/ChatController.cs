using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, IFileUploadService fileUploadService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Get chat history between current user and another user
    /// </summary>
    [HttpGet("history/{otherUserId}")]
    public async Task<IActionResult> GetHistory(long otherUserId)
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            var messages = await _chatService.GetChatHistoryAsync(currentUserId, otherUserId);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history");
            return StatusCode(500, new { error = "Failed to load chat history" });
        }
    }

    /// <summary>
    /// Get all conversations for current user
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            var conversations = await _chatService.GetConversationsAsync(currentUserId);
            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations");
            return StatusCode(500, new { error = "Failed to load conversations" });
        }
    }

    /// <summary>
    /// Mark messages from another user as read
    /// </summary>
    [HttpPost("mark-read/{otherUserId}")]
    public async Task<IActionResult> MarkRead(long otherUserId)
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            await _chatService.MarkMessagesAsReadAsync(currentUserId, otherUserId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
            return StatusCode(500, new { error = "Failed to mark messages as read" });
        }
    }

    /// <summary>
    /// Get total unread message count for current user
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            var count = await _chatService.GetUnreadCountAsync(currentUserId);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return Ok(new { count = 0 });
        }
    }

    /// <summary>
    /// Send a message to another user
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { error = "Message content cannot be empty" });
            }

            var message = await _chatService.SendMessageAsync(currentUserId, request.ReceiverId, request.Content);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new { error = "Failed to send message" });
        }
    }

    /// <summary>
    /// Delete a message (only sender can delete)
    /// </summary>
    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(long messageId)
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            var success = await _chatService.DeleteMessageAsync(messageId, currentUserId);
            if (success)
            {
                return Ok(new { success = true });
            }

            return NotFound(new { error = "Message not found or you don't have permission to delete it" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message");
            return StatusCode(500, new { error = "Failed to delete message" });
        }
    }

    /// <summary>
    /// Search messages by content
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchMessages([FromQuery] string query)
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Search query cannot be empty" });
            }

            var messages = await _chatService.SearchMessagesAsync(currentUserId, query);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages");
            return StatusCode(500, new { error = "Failed to search messages" });
        }
    }

    /// <summary>
    /// Upload file and send as message
    /// </summary>
    [HttpPost("upload")]
    [IgnoreAntiforgeryToken] // Allow file upload without antiforgery token in header
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] long receiverId, [FromForm] string? message)
    {
        try
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(currentUserIdStr, out long currentUserId))
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file provided" });
            }

            // Determine upload type based on file
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uploadType = extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" => "images",
                ".mp4" or ".webm" or ".mov" or ".avi" => "videos",
                ".mp3" or ".wav" or ".ogg" or ".m4a" => "audio",
                _ => "files"
            };

            // Upload file
            var (success, filePath, thumbnailPath, errorMessage) = await _fileUploadService.UploadFileAsync(
                file, uploadType, currentUserId);

            if (!success || filePath == null)
            {
                _logger.LogError("File upload failed: {Error}", errorMessage);
                return BadRequest(new { error = errorMessage ?? "Failed to upload file" });
            }

            // Send message with attachment
            var messageDto = await _chatService.SendMessageWithAttachmentAsync(
                currentUserId,
                receiverId,
                message ?? string.Empty, // Ensure content is never null
                filePath,
                uploadType,
                file.FileName,
                file.Length,
                thumbnailPath
            );

            _logger.LogInformation("File uploaded and message sent: {FilePath}", filePath);
            return Ok(messageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "Failed to upload file: " + ex.Message });
        }
    }
}
