namespace MoneyTracker.Models.DTOs
{
    public class AdvancedSearchDto
    {
        public string? Query { get; set; }
        public string? Type { get; set; } // "expense", "income", "all"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public List<long>? CategoryIds { get; set; }
        public List<string>? Categories { get; set; }
        public string? SortBy { get; set; } // "date", "amount", "category", "created"
        public string? SortOrder { get; set; } // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? HasNote { get; set; }
        public string? Currency { get; set; }
    }

    public class SearchResultDto
    {
        public List<TransactionSearchResult> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public Dictionary<string, object> Facets { get; set; } = new();
    }

    public class TransactionSearchResult
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty; // "expense" or "income"
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string HighlightedNote { get; set; } = string.Empty;
    }
}
