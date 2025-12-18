using System;

namespace MoneyTrackerApp.Models
{
    public class UserOtp
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string OtpCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
