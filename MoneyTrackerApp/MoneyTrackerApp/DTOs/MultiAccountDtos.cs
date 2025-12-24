namespace MoneyTrackerApp.DTOs
{
    public class UserAccountSummaryDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public Guid SessionId { get; set; }
        public bool IsActive { get; set; }
    }
}
