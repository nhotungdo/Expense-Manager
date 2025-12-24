using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrackerApp.Models
{
    public class AiSuggestion
    {
        [Key]
        public int Id { get; set; }

        public long UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [StringLength(1024)]
        public string Suggestion { get; set; } = string.Empty;

        [StringLength(50)]
        public string SuggestionType { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
