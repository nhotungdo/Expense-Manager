namespace MoneyTrackerApp.DTOs
{
    public class AnalysisResultDto
    {
        public string Trend { get; set; } = "Stable";
        public decimal TrendPercentage { get; set; }
        public decimal TotalSpendingThisMonth { get; set; }
        public decimal PredictedSpendingThisMonth { get; set; }
        public List<string> Insights { get; set; } = new List<string>();
        public List<AnomalyDto> Anomalies { get; set; } = new List<AnomalyDto>();
    }

    public class AnomalyDto
    {
        public long TransactionId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string CategoryName { get; set; }
        public string Note { get; set; }
        public string Reason { get; set; }
    }
}
