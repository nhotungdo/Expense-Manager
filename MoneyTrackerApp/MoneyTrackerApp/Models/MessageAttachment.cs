using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrackerApp.Models;

public class MessageAttachment
{
    [Key]
    public long Id { get; set; }

    public long MessageId { get; set; }

    [Required]
    [MaxLength(512)]
    public string AttachmentUrl { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string AttachmentType { get; set; } = null!; // image, file, video, audio

    [Required]
    [MaxLength(256)]
    public string FileName { get; set; } = null!;

    public long FileSize { get; set; }

    [MaxLength(100)]
    public string? MimeType { get; set; }

    [MaxLength(512)]
    public string? ThumbnailUrl { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("MessageId")]
    public virtual Message Message { get; set; } = null!;
}
