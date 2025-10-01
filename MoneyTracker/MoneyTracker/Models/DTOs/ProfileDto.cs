namespace MoneyTracker.Models.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string Language { get; set; } = "vi";
        public string DefaultCurrency { get; set; } = "VND";
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
        public string Theme { get; set; } = "light";
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class DeleteAccountDto
    {
        public string Password { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ProfileStatsDto
    {
        public int TotalTransactions { get; set; }
        public int TotalCategories { get; set; }
        public int AiSuggestions { get; set; }
        public int AccountAge { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
