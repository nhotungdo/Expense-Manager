using System;

namespace MoneyTrackerApp.DTOs
{
    public class TransactionAnalysisDto
    {
        public string Date { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
    }
}
