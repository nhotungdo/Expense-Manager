using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID");
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCategories([FromQuery] int? type = null)
        {
            try
            {
                var userId = GetUserId();
                var categories = await _categoryService.GetUserCategoriesAsync(userId, type);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving categories", details = ex.Message });
            }
        }
    }
}
