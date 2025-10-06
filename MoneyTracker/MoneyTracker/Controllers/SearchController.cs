using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MoneyTracker.Models.DTOs;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly IAdvancedSearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(IAdvancedSearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> SearchTransactions([FromBody] AdvancedSearchDto searchDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var result = await _searchService.SearchTransactionsAsync(userId.Value, searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching transactions");
                return StatusCode(500, "Error searching transactions");
            }
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string query, [FromQuery] string type = "all")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (string.IsNullOrEmpty(query))
                {
                    return Ok(new List<string>());
                }

                var suggestions = await _searchService.GetSearchSuggestionsAsync(userId.Value, query, type);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions");
                return StatusCode(500, "Error getting search suggestions");
            }
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetSearchFilters()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var filters = await _searchService.GetSearchFiltersAsync(userId.Value);
                return Ok(filters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search filters");
                return StatusCode(500, "Error getting search filters");
            }
        }

        [HttpGet("quick")]
        public async Task<IActionResult> QuickSearch([FromQuery] string q, [FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized();

                if (string.IsNullOrEmpty(q))
                {
                    return Ok(new SearchResultDto());
                }

                var searchDto = new AdvancedSearchDto
                {
                    Query = q,
                    Page = 1,
                    PageSize = limit,
                    SortBy = "date",
                    SortOrder = "desc"
                };

                var result = await _searchService.SearchTransactionsAsync(userId.Value, searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing quick search");
                return StatusCode(500, "Error performing quick search");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
