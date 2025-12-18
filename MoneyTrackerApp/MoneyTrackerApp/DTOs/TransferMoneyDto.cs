using System.ComponentModel.DataAnnotations;

namespace MoneyTrackerApp.DTOs
{
    public class TransferMoneyDto
    {
        [Required]
        public long SourceAccountId { get; set; }

        [Required]
        public long TargetAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string? Note { get; set; }
        
        public string? OtpCode { get; set; }
    }
}
