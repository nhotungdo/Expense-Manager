namespace MoneyTracker.Models.DTOs
{
    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Role { get; set; } = "USER";
        public bool Enabled { get; set; } = true;
    }


    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsers { get; set; }
        public int TotalAISuggestions { get; set; }
        public List<UserRegistrationData> UserRegistrationData { get; set; } = new();
        public List<SystemActivityData> SystemActivityData { get; set; } = new();
    }

    public class UserRegistrationData
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SystemActivityData
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
