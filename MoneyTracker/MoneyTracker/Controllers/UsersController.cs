using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.DTOs.Auth;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUnitOfWork unitOfWork, IAuthService authService, ILogger<UsersController> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                FullName = user.FullName ?? "",
                ProfilePictureUrl = user.ProfilePictureUrl ?? "",
                OnboardingCompleted = user.OnboardingCompleted,
                Role = user.Role,
                Enabled = user.Enabled,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                Language = user.Language,
                DefaultCurrency = user.DefaultCurrency,
                Timezone = user.Timezone,
                Theme = user.Theme
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateUserRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.FullName = request.FullName;
            user.ProfilePictureUrl = request.ProfilePictureUrl;
            user.Language = request.Language;
            user.DefaultCurrency = request.DefaultCurrency;
            user.Timezone = request.Timezone;
            user.Theme = request.Theme;
            user.EmailNotifications = request.EmailNotifications;
            user.PushNotifications = request.PushNotifications;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                FullName = user.FullName ?? "",
                ProfilePictureUrl = user.ProfilePictureUrl ?? "",
                OnboardingCompleted = user.OnboardingCompleted,
                Role = user.Role,
                Enabled = user.Enabled,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                Language = user.Language,
                DefaultCurrency = user.DefaultCurrency,
                Timezone = user.Timezone,
                Theme = user.Theme
            };

            _logger.LogInformation("User {UserId} updated their profile", userId);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("complete-onboarding")]
    public async Task<ActionResult> CompleteOnboarding()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.OnboardingCompleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} completed onboarding", userId);
            return Ok(new { message = "Onboarding completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing onboarding");
            return StatusCode(500, "Internal server error");
        }
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }
}

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string Language { get; set; } = "vi";
    public string DefaultCurrency { get; set; } = "VND";
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string Theme { get; set; } = "light";
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
}
