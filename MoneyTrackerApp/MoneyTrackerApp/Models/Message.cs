using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrackerApp.Models;

public class Message
{
    [Key]
    public long Id { get; set; }

    public long SenderId { get; set; }

    public long ReceiverId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public bool IsRead { get; set; }

    [MaxLength(512)]
    public string? AttachmentUrl { get; set; }

    [MaxLength(50)]
    public string? AttachmentType { get; set; }

    [MaxLength(256)]
    public string? AttachmentName { get; set; }

    public long? AttachmentSize { get; set; }

    [MaxLength(512)]
    public string? ThumbnailUrl { get; set; }

    [ForeignKey("SenderId")]
    public virtual User Sender { get; set; } = null!;

    [ForeignKey("ReceiverId")]
    public virtual User Receiver { get; set; } = null!;
}
