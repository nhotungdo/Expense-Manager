using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MoneyTracker.Services;

namespace MoneyTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OnboardingController : ControllerBase
    {
        private readonly IDefaultCategoryService _defaultCategoryService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            IDefaultCategoryService defaultCategoryService,
            ILogger<OnboardingController> logger)
        {
            _defaultCategoryService = defaultCategoryService;
            _logger = logger;
        }

        [HttpGet("check-setup")]
        public async Task<IActionResult> CheckSetupStatus()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var hasCategories = await _defaultCategoryService.HasUserCategoriesAsync(userId.Value);

                return Ok(new
                {
                    HasCategories = hasCategories,
                    NeedsSetup = !hasCategories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking setup status for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("setup-default-categories")]
        public async Task<IActionResult> SetupDefaultCategories()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                await _defaultCategoryService.SetupDefaultCategoriesAsync(userId.Value);

                _logger.LogInformation("Default categories set up for user {UserId}", userId);

                return Ok(new { message = "Đã thiết lập danh mục mặc định thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up default categories for user {UserId}", userId);
                return StatusCode(500, "Có lỗi xảy ra khi thiết lập danh mục mặc định");
            }
        }

        [HttpGet("default-categories")]
        public async Task<IActionResult> GetDefaultCategories()
        {
            try
            {
                var expenseCategories = await _defaultCategoryService.GetDefaultExpenseCategoriesAsync();
                var incomeCategories = await _defaultCategoryService.GetDefaultIncomeCategoriesAsync();

                return Ok(new
                {
                    ExpenseCategories = expenseCategories.Select(c => new { c.Name, c.Description }),
                    IncomeCategories = incomeCategories.Select(c => new { c.Name, c.Description })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default categories");
                return StatusCode(500, "Internal server error");
            }
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
