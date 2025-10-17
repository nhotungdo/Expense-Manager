using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Models;

namespace MoneyTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ExpenseManagerContext _db;
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _passwordHasher = new();

    public UsersController(ExpenseManagerContext db)
    {
        _db = db;
    }

    public record UpdateMeRequest(string? FirstName, string? LastName, DateOnly? DateOfBirth, string? Address, string? ProfilePictureUrl, string? Language, string? DefaultCurrency, string? Theme, bool? EmailNotifications, bool? PushNotifications);

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        if (!long.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.DateOfBirth,
            user.Address,
            user.ProfilePictureUrl,
            user.Language,
            user.DefaultCurrency,
            user.Theme,
            user.EmailNotifications,
            user.PushNotifications
        });
    }

    [Authorize]
    [HttpPut("me")]
    public IActionResult UpdateMe([FromBody] UpdateMeRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        if (!long.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound();

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.FullName = string.Join(' ', new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;
        user.Address = request.Address ?? user.Address;
        user.ProfilePictureUrl = request.ProfilePictureUrl ?? user.ProfilePictureUrl;
        user.Language = request.Language ?? user.Language;
        user.DefaultCurrency = request.DefaultCurrency ?? user.DefaultCurrency;
        user.Theme = request.Theme ?? user.Theme;
        if (request.EmailNotifications.HasValue) user.EmailNotifications = request.EmailNotifications.Value;
        if (request.PushNotifications.HasValue) user.PushNotifications = request.PushNotifications.Value;
        user.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();
        return NoContent();
    }

    public record ChangePasswordRequest(string OldPassword, string NewPassword);

    [Authorize]
    [HttpPut("me/password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        if (!long.TryParse(userIdClaim, out var userId)) return Unauthorized();
        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound();
        if (string.IsNullOrEmpty(user.PasswordHash)) return BadRequest(new { error = "Password not set for this account" });

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
        if (verify == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        {
            return BadRequest(new { error = "Old password incorrect" });
        }
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return BadRequest(new { error = "New password too short" });
        }
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return NoContent();
    }
}


