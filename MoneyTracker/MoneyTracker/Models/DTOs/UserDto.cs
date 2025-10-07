namespace MoneyTracker.Models.DTOs
{
    public class UserDto
    {
        public long Id { get; set; }
        public string GoogleId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string Language { get; set; } = "vi";
        public string DefaultCurrency { get; set; } = "VND";
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
        public string Theme { get; set; } = "light";
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PictureUrl { get; set; }
        public string Role { get; set; } = "USER";
    }


    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? Enabled { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
