using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Games
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IChallengeService _challengeService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IChallengeService challengeService, ILogger<IndexModel> logger)
        {
            _challengeService = challengeService;
            _logger = logger;
        }

        public List<UserChallenge> MyChallenges { get; set; } = new();
        public List<Challenge> AvailableChallenges { get; set; } = new();
        public List<LeaderboardEntryDto> Leaderboard { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    _logger.LogWarning("User ID not found in claims");
                    return RedirectToPage("/Account/Login");
                }

                // Load data in parallel for better performance
                var myChallengesTask = _challengeService.GetUserChallengesAsync(userId);
                var availableChallengesTask = _challengeService.GetAvailableChallengesAsync(userId);
                var leaderboardTask = _challengeService.GetLeaderboardAsync(userId);

                await Task.WhenAll(myChallengesTask, availableChallengesTask, leaderboardTask);

                MyChallenges = await myChallengesTask;
                AvailableChallenges = await availableChallengesTask;
                Leaderboard = await leaderboardTask;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading games page for user");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu. Vui lòng thử lại.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostJoinAsync(long challengeId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    _logger.LogWarning("User ID not found in claims during join challenge");
                    return RedirectToPage("/Account/Login");
                }

                if (challengeId <= 0)
                {
                    TempData["ErrorMessage"] = "Thử thách không hợp lệ.";
                    return RedirectToPage();
                }

                await _challengeService.JoinChallengeAsync(userId, challengeId);
                TempData["SuccessMessage"] = "Đã tham gia thử thách thành công! Hãy cố gắng hoàn thành nhé!";
                
                _logger.LogInformation("User {UserId} joined challenge {ChallengeId}", userId, challengeId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User {UserId} attempted to join challenge {ChallengeId}", GetUserId(), challengeId);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining challenge {ChallengeId} for user {UserId}", challengeId, GetUserId());
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tham gia thử thách. Vui lòng thử lại.";
            }
            
            return RedirectToPage();
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
