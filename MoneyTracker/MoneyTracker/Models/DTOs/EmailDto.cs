namespace MoneyTracker.Models.DTOs
{
    public class EmailDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UserEmail { get; set; } = string.Empty; // For display purposes
    }

    public class CreateEmailDto
    {
        public long UserId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class UpdateEmailDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class EmailFilterDto
    {
        public long? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? SentFrom { get; set; }
        public DateTime? SentTo { get; set; }
        public string? SearchTerm { get; set; }
    }
}
