using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.DTOs.Auth;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<LoginResponse>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("Token is required");
            }

            var user = await _authService.AuthenticateGoogleUserAsync(request.Token);
            if (user == null)
            {
                return Unauthorized("Invalid Google token");
            }

            var token = await _authService.GenerateJwtTokenAsync(user);
            var isNewUser = user.CreatedAt?.Date == DateTime.UtcNow.Date;

            var response = new LoginResponse
            {
                Token = token,
                User = new UserDto
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
                },
                IsNewUser = isNewUser
            };

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("validate-token")]
    public async Task<ActionResult<bool>> ValidateToken([FromBody] string token)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, "Internal server error");
        }
    }
}
