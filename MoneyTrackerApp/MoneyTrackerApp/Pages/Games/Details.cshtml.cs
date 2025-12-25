using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Games
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IChallengeService _challengeService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IChallengeService challengeService, ILogger<DetailsModel> logger)
        {
            _challengeService = challengeService;
            _logger = logger;
        }

        public UserChallenge UserChallenge { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(long id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    _logger.LogWarning("User ID not found in claims");
                    return RedirectToPage("/Account/Login");
                }

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid challenge ID: {ChallengeId}", id);
                    TempData["ErrorMessage"] = "ID thử thách không hợp lệ.";
                    return RedirectToPage("./Index");
                }

                var challenge = await _challengeService.GetUserChallengeAsync(userId, id);

                if (challenge == null)
                {
                    _logger.LogWarning("Challenge not found or user not joined. UserId: {UserId}, ChallengeId: {ChallengeId}", userId, id);
                    TempData["ErrorMessage"] = "Không tìm thấy thử thách hoặc bạn chưa tham gia.";
                    return RedirectToPage("./Index");
                }

                if (challenge.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized access attempt. UserId: {UserId}, ChallengeUserId: {ChallengeUserId}", userId, challenge.UserId);
                    TempData["ErrorMessage"] = "Bạn không có quyền xem thử thách này.";
                    return RedirectToPage("./Index");
                }

                UserChallenge = challenge;
                
                _logger.LogInformation("User {UserId} viewing challenge {ChallengeId}", userId, id);
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading challenge details. ChallengeId: {ChallengeId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin thử thách. Vui lòng thử lại.";
                return RedirectToPage("./Index");
            }
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
