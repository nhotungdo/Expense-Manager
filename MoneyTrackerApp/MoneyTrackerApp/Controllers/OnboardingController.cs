using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;
using MoneyTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace MoneyTrackerApp.Controllers;

/// <summary>
/// API Controller for onboarding flow
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OnboardingController : ControllerBase
{
    private readonly OnboardingService _onboardingService;
    private readonly ILogger<OnboardingController> _logger;
    private readonly MoneyTrackerApp.Services.JwtTokenService _jwtService;
    private readonly ExpenseManagerContext _context;

    public OnboardingController(
        OnboardingService onboardingService,
        ILogger<OnboardingController> logger,
        MoneyTrackerApp.Services.JwtTokenService jwtService,
        ExpenseManagerContext context)
    {
        _onboardingService = onboardingService;
        _logger = logger;
        _jwtService = jwtService;
        _context = context;
    }

    /// <summary>
    /// Get current onboarding status
    /// </summary>
    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var status = await _onboardingService.GetOnboardingStatusAsync(userId);
            
            if (status == null)
            {
                // Initialize onboarding if not exists
                status = await _onboardingService.InitializeOnboardingAsync(userId);
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting onboarding status: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Initialize onboarding
    /// </summary>
    [HttpPost("initialize")]
    [Authorize]
    public async Task<IActionResult> Initialize()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var status = await _onboardingService.InitializeOnboardingAsync(userId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error initializing onboarding: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update onboarding step
    /// </summary>
    [HttpPut("step")]
    [Authorize]
    public async Task<IActionResult> UpdateStep([FromBody] UpdateOnboardingStepDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var success = await _onboardingService.UpdateStepAsync(userId, dto.Step, dto.StepData);
            
            if (!success)
            {
                return NotFound(new { message = "Onboarding status not found" });
            }

            return Ok(new { message = "Step updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating onboarding step: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Complete onboarding
    /// </summary>
    [HttpPost("complete")]
    [Authorize]
    public async Task<IActionResult> Complete([FromBody] CompleteOnboardingDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var (success, message) = await _onboardingService.CompleteOnboardingAsync(userId, dto);
            
            if (!success)
            {
                return BadRequest(new { message = $"Failed to complete onboarding: {message}" });
            }

            // Issue new tokens with OnboardingCompleted = true
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                var pair = await _jwtService.IssueAsync(user);
                
                Response.Cookies.Append("AccessToken", pair.access, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                });

                return Ok(new { message = "Onboarding completed successfully", accessToken = pair.access, refreshToken = pair.refresh });
            }

            return Ok(new { message = "Onboarding completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error completing onboarding: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get category template preview
    /// </summary>
    [HttpGet("templates/{template}")]
    public IActionResult GetTemplatePreview(string template)
    {
        try
        {
            var categories = _onboardingService.GetCategoryTemplatePreview(template);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting template preview: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Calculate monthly savings
    /// </summary>
    [HttpPost("calculate-savings")]
    public IActionResult CalculateSavings([FromBody] OnboardingSavingsGoalDto dto)
    {
        try
        {
            if (!dto.TargetAmount.HasValue || !dto.TargetDate.HasValue)
            {
                return BadRequest(new { message = "Target amount and date are required" });
            }

            var monthlyAmount = _onboardingService.CalculateMonthlySavings(
                dto.TargetAmount.Value, 
                DateOnly.FromDateTime(dto.TargetDate.Value));

            return Ok(new { monthlyAmount });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calculating savings: {ex.Message}");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
