using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;
using MoneyTracker.Models.DTOs;
using System.Text.RegularExpressions;

namespace MoneyTracker.Services
{
    public class AdvancedSearchService : IAdvancedSearchService
    {
        private readonly ExpenseManagerContext _context;
        private readonly ILogger<AdvancedSearchService> _logger;

        public AdvancedSearchService(ExpenseManagerContext context, ILogger<AdvancedSearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SearchResultDto> SearchTransactionsAsync(long userId, AdvancedSearchDto searchDto)
        {
            try
            {
                var query = _context.Expenses
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Category)
                    .Select(e => new TransactionSearchResult
                    {
                        Id = e.Id,
                        Type = "expense",
                        Amount = e.Amount,
                        Currency = e.Currency ?? "VND",
                        Date = e.ExpenseDate.ToDateTime(TimeOnly.MinValue),
                        Note = e.Note,
                        CategoryName = e.Category != null ? e.Category.Name : "Uncategorized",
                        CategoryId = e.CategoryId,
                        CreatedAt = e.CreatedAt ?? DateTime.UtcNow
                    })
                    .AsQueryable();

                var incomeQuery = _context.Incomes
                    .Where(i => i.UserId == userId)
                    .Include(i => i.Category)
                    .Select(i => new TransactionSearchResult
                    {
                        Id = i.Id,
                        Type = "income",
                        Amount = i.Amount,
                        Currency = i.Currency ?? "VND",
                        Date = i.IncomeDate.ToDateTime(TimeOnly.MinValue),
                        Note = i.Note,
                        CategoryName = i.Category != null ? i.Category.Name : "Uncategorized",
                        CategoryId = i.CategoryId,
                        CreatedAt = i.CreatedAt ?? DateTime.UtcNow
                    })
                    .AsQueryable();

                // Combine queries based on type filter
                IQueryable<TransactionSearchResult> combinedQuery;
                if (searchDto.Type?.ToLower() == "expense")
                {
                    combinedQuery = query;
                }
                else if (searchDto.Type?.ToLower() == "income")
                {
                    combinedQuery = incomeQuery;
                }
                else
                {
                    combinedQuery = query.Concat(incomeQuery);
                }

                // Apply filters
                if (searchDto.StartDate.HasValue)
                {
                    combinedQuery = combinedQuery.Where(t => t.Date >= searchDto.StartDate.Value);
                }

                if (searchDto.EndDate.HasValue)
                {
                    combinedQuery = combinedQuery.Where(t => t.Date <= searchDto.EndDate.Value);
                }

                if (searchDto.MinAmount.HasValue)
                {
                    combinedQuery = combinedQuery.Where(t => t.Amount >= searchDto.MinAmount.Value);
                }

                if (searchDto.MaxAmount.HasValue)
                {
                    combinedQuery = combinedQuery.Where(t => t.Amount <= searchDto.MaxAmount.Value);
                }

                if (searchDto.CategoryIds != null && searchDto.CategoryIds.Any())
                {
                    combinedQuery = combinedQuery.Where(t => t.CategoryId.HasValue && searchDto.CategoryIds.Contains(t.CategoryId.Value));
                }

                if (searchDto.Categories != null && searchDto.Categories.Any())
                {
                    combinedQuery = combinedQuery.Where(t => searchDto.Categories.Contains(t.CategoryName));
                }

                if (searchDto.HasNote.HasValue)
                {
                    if (searchDto.HasNote.Value)
                    {
                        combinedQuery = combinedQuery.Where(t => !string.IsNullOrEmpty(t.Note));
                    }
                    else
                    {
                        combinedQuery = combinedQuery.Where(t => string.IsNullOrEmpty(t.Note));
                    }
                }

                if (!string.IsNullOrEmpty(searchDto.Currency))
                {
                    combinedQuery = combinedQuery.Where(t => t.Currency == searchDto.Currency);
                }

                // Apply text search
                if (!string.IsNullOrEmpty(searchDto.Query))
                {
                    var searchTerm = searchDto.Query.ToLower();
                    combinedQuery = combinedQuery.Where(t =>
                        (t.Note != null && t.Note.ToLower().Contains(searchTerm)) ||
                        t.CategoryName.ToLower().Contains(searchTerm));
                }

                // Get total count before pagination
                var totalCount = await combinedQuery.CountAsync();

                // Apply sorting
                combinedQuery = searchDto.SortBy?.ToLower() switch
                {
                    "amount" => searchDto.SortOrder?.ToLower() == "asc"
                        ? combinedQuery.OrderBy(t => t.Amount)
                        : combinedQuery.OrderByDescending(t => t.Amount),
                    "category" => searchDto.SortOrder?.ToLower() == "asc"
                        ? combinedQuery.OrderBy(t => t.CategoryName)
                        : combinedQuery.OrderByDescending(t => t.CategoryName),
                    "created" => searchDto.SortOrder?.ToLower() == "asc"
                        ? combinedQuery.OrderBy(t => t.CreatedAt)
                        : combinedQuery.OrderByDescending(t => t.CreatedAt),
                    _ => searchDto.SortOrder?.ToLower() == "asc"
                        ? combinedQuery.OrderBy(t => t.Date)
                        : combinedQuery.OrderByDescending(t => t.Date)
                };

                // Apply pagination
                var transactions = await combinedQuery
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                // Add highlighting for search results
                if (!string.IsNullOrEmpty(searchDto.Query))
                {
                    foreach (var transaction in transactions)
                    {
                        transaction.HighlightedNote = HighlightSearchTerm(transaction.Note ?? "", searchDto.Query);
                    }
                }

                // Get facets for filtering
                var facets = await GetSearchFacetsAsync(userId, searchDto);

                return new SearchResultDto
                {
                    Transactions = transactions,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    Facets = facets
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching transactions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(long userId, string query, string type = "all")
        {
            try
            {
                if (string.IsNullOrEmpty(query) || query.Length < 2)
                {
                    return new List<string>();
                }

                var searchTerm = query.ToLower();
                var suggestions = new List<string>();

                // Get note suggestions
                var noteSuggestions = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Note != null && e.Note.ToLower().Contains(searchTerm))
                    .Select(e => e.Note)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                suggestions.AddRange(noteSuggestions.Where(n => n != null).Cast<string>());

                // Get income note suggestions
                var incomeNoteSuggestions = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Note != null && i.Note.ToLower().Contains(searchTerm))
                    .Select(i => i.Note)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                suggestions.AddRange(incomeNoteSuggestions.Where(n => n != null).Cast<string>());

                // Get category suggestions
                var categorySuggestions = await _context.Categories
                    .Where(c => (c.UserId == userId || c.UserId == null) && c.Name.ToLower().Contains(searchTerm))
                    .Select(c => c.Name)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                suggestions.AddRange(categorySuggestions);

                return suggestions.Distinct().Take(20).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, object>> GetSearchFiltersAsync(long userId)
        {
            try
            {
                var filters = new Dictionary<string, object>();

                // Get date range
                var minDate = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .MinAsync(e => (DateTime?)e.ExpenseDate.ToDateTime(TimeOnly.MinValue));

                var maxDate = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .MaxAsync(e => (DateTime?)e.ExpenseDate.ToDateTime(TimeOnly.MinValue));

                var incomeMinDate = await _context.Incomes
                    .Where(i => i.UserId == userId)
                    .MinAsync(i => (DateTime?)i.IncomeDate.ToDateTime(TimeOnly.MinValue));

                var incomeMaxDate = await _context.Incomes
                    .Where(i => i.UserId == userId)
                    .MaxAsync(i => (DateTime?)i.IncomeDate.ToDateTime(TimeOnly.MinValue));

                if (minDate.HasValue && incomeMinDate.HasValue)
                {
                    filters["minDate"] = minDate.Value < incomeMinDate.Value ? minDate.Value : incomeMinDate.Value;
                }
                else if (minDate.HasValue)
                {
                    filters["minDate"] = minDate.Value;
                }
                else if (incomeMinDate.HasValue)
                {
                    filters["minDate"] = incomeMinDate.Value;
                }

                if (maxDate.HasValue && incomeMaxDate.HasValue)
                {
                    filters["maxDate"] = maxDate.Value > incomeMaxDate.Value ? maxDate.Value : incomeMaxDate.Value;
                }
                else if (maxDate.HasValue)
                {
                    filters["maxDate"] = maxDate.Value;
                }
                else if (incomeMaxDate.HasValue)
                {
                    filters["maxDate"] = incomeMaxDate.Value;
                }

                // Get amount range
                var minAmount = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .MinAsync(e => (decimal?)e.Amount);

                var maxAmount = await _context.Expenses
                    .Where(e => e.UserId == userId)
                    .MaxAsync(e => (decimal?)e.Amount);

                var incomeMinAmount = await _context.Incomes
                    .Where(i => i.UserId == userId)
                    .MinAsync(i => (decimal?)i.Amount);

                var incomeMaxAmount = await _context.Incomes
                    .Where(i => i.UserId == userId)
                    .MaxAsync(i => (decimal?)i.Amount);

                if (minAmount.HasValue && incomeMinAmount.HasValue)
                {
                    filters["minAmount"] = minAmount.Value < incomeMinAmount.Value ? minAmount.Value : incomeMinAmount.Value;
                }
                else if (minAmount.HasValue)
                {
                    filters["minAmount"] = minAmount.Value;
                }
                else if (incomeMinAmount.HasValue)
                {
                    filters["minAmount"] = incomeMinAmount.Value;
                }

                if (maxAmount.HasValue && incomeMaxAmount.HasValue)
                {
                    filters["maxAmount"] = maxAmount.Value > incomeMaxAmount.Value ? maxAmount.Value : incomeMaxAmount.Value;
                }
                else if (maxAmount.HasValue)
                {
                    filters["maxAmount"] = maxAmount.Value;
                }
                else if (incomeMaxAmount.HasValue)
                {
                    filters["maxAmount"] = incomeMaxAmount.Value;
                }

                // Get categories
                var categories = await _context.Categories
                    .Where(c => c.UserId == userId || c.UserId == null)
                    .Select(c => new { c.Id, c.Name, c.Type })
                    .ToListAsync();

                filters["categories"] = categories;

                // Get currencies
                var currencies = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Currency != null)
                    .Select(e => e.Currency)
                    .Distinct()
                    .ToListAsync();

                var incomeCurrencies = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Currency != null)
                    .Select(i => i.Currency)
                    .Distinct()
                    .ToListAsync();

                currencies.AddRange(incomeCurrencies);
                filters["currencies"] = currencies.Distinct().ToList();

                return filters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search filters for user {UserId}", userId);
                return new Dictionary<string, object>();
            }
        }

        private async Task<Dictionary<string, object>> GetSearchFacetsAsync(long userId, AdvancedSearchDto searchDto)
        {
            var facets = new Dictionary<string, object>();

            // Get category distribution
            var categoryDistribution = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .GroupBy(e => e.Category != null ? e.Category.Name : "Uncategorized")
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            facets["categoryDistribution"] = categoryDistribution;

            // Get monthly distribution
            var monthlyDistribution = await _context.Expenses
                .Where(e => e.UserId == userId)
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count(),
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Month)
                .ToDictionaryAsync(x => x.Month, x => new { x.Count, x.Amount });

            facets["monthlyDistribution"] = monthlyDistribution;

            return facets;
        }

        private string HighlightSearchTerm(string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
            {
                return text;
            }

            var pattern = Regex.Escape(searchTerm);
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.Replace(text, $"<mark>{searchTerm}</mark>");
        }
    }
}
