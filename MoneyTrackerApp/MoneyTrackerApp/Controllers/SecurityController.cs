using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ExpenseManagerContext _context;

    public SecurityController(ExpenseManagerContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get 2FA status
    /// </summary>
    [HttpGet("2fa/status")]
    public async Task<ActionResult> Get2FAStatus()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var method = user.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == "TwoFactorMethod")?.ClaimValue ?? "none";

        return Ok(new
        {
            enabled = user.TwoFactorEnabled,
            method = method
        });
    }

    /// <summary>
    /// Enable 2FA
    /// </summary>
    [HttpPost("2fa/enable")]
    public async Task<ActionResult> Enable2FA([FromBody] Enable2FADto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        if (user.TwoFactorEnabled)
            return BadRequest(new { message = "2FA is already enabled" });

        // Generate secret key for TOTP
        var secretKey = GenerateSecretKey();
        var qrCodeUrl = GenerateQRCodeUrl(user.Email!, secretKey);

        // Store secret and method in claims
        UpdateOrAddClaim(user, "TwoFactorSecret", secretKey);
        UpdateOrAddClaim(user, "TwoFactorMethod", dto.Method);
        
        user.TwoFactorEnabled = false; // Will be enabled after verification
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            secretKey = secretKey,
            qrCodeUrl = qrCodeUrl,
            message = "Scan the QR code with your authenticator app and verify to complete setup"
        });
    }

    /// <summary>
    /// Verify and complete 2FA setup
    /// </summary>
    [HttpPost("2fa/verify")]
    public async Task<ActionResult> Verify2FA([FromBody] Verify2FADto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var secret = user.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == "TwoFactorSecret")?.ClaimValue;

        if (string.IsNullOrEmpty(secret))
            return BadRequest(new { message = "2FA setup not initiated" });

        // Verify TOTP code
        if (!VerifyTOTP(secret, dto.Code))
            return BadRequest(new { message = "Invalid verification code" });

        // Enable 2FA
        user.TwoFactorEnabled = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "2FA enabled successfully" });
    }

    /// <summary>
    /// Disable 2FA
    /// </summary>
    [HttpPost("2fa/disable")]
    public async Task<ActionResult> Disable2FA([FromBody] Disable2FADto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.AspNetUserClaims)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        if (!user.TwoFactorEnabled)
            return BadRequest(new { message = "2FA is not enabled" });

        // Verify password before disabling
        if (!string.IsNullOrEmpty(user.PasswordHash) && !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return BadRequest(new { message = "Invalid password" });

        user.TwoFactorEnabled = false;
        
        // Remove claims
        var secretClaim = user.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == "TwoFactorSecret");
        if (secretClaim != null) _context.AspNetUserClaims.Remove(secretClaim);

        var methodClaim = user.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == "TwoFactorMethod");
        if (methodClaim != null) _context.AspNetUserClaims.Remove(methodClaim);

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "2FA disabled successfully" });
    }

    /// <summary>
    /// Get active sessions/devices
    /// </summary>
    [HttpGet("sessions")]
    public async Task<ActionResult> GetActiveSessions()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get user's active tokens from AspNetUserTokens
        // We assume Name format is "RefreshToken:{DeviceId}"
        var tokens = await _context.AspNetUserTokens
            .Where(t => t.UserId == userId && t.LoginProvider == "MoneyTrackerApp" && t.Name.StartsWith("RefreshToken:"))
            .ToListAsync();

        var sessions = tokens.Select(t => new
        {
            id = t.Name.Replace("RefreshToken:", ""),
            deviceInfo = "Device " + t.Name.Replace("RefreshToken:", ""), // Simplified
            lastUsed = DateTime.UtcNow // We don't track last used in this simple schema
        });

        return Ok(sessions);
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpDelete("sessions/{deviceId}")]
    public async Task<ActionResult> RevokeSession(string deviceId)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var token = await _context.AspNetUserTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.LoginProvider == "MoneyTrackerApp" && t.Name == $"RefreshToken:{deviceId}");

        if (token == null)
            return NotFound(new { message = "Session not found" });

        _context.AspNetUserTokens.Remove(token);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Session revoked successfully" });
    }

    /// <summary>
    /// Revoke all other sessions
    /// </summary>
    [HttpPost("sessions/revoke-all")]
    public async Task<ActionResult> RevokeAllOtherSessions()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // In a real app we'd identify current session ID. Here we just revoke all for simplicity or would need to pass current device ID
        var tokens = await _context.AspNetUserTokens
            .Where(t => t.UserId == userId && t.LoginProvider == "MoneyTrackerApp" && t.Name.StartsWith("RefreshToken:"))
            .ToListAsync();

        _context.AspNetUserTokens.RemoveRange(tokens);

        // Update security stamp to invalidate all access tokens
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "All sessions revoked successfully" });
    }

    // Helper methods

    private void UpdateOrAddClaim(User user, string type, string value)
    {
        var claim = user.AspNetUserClaims.FirstOrDefault(c => c.ClaimType == type);
        if (claim != null)
        {
            claim.ClaimValue = value;
        }
        else
        {
            user.AspNetUserClaims.Add(new AspNetUserClaim
            {
                UserId = user.Id,
                ClaimType = type,
                ClaimValue = value
            });
        }
    }

    private string GenerateSecretKey()
    {
        var bytes = new byte[20];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
    }

    private string GenerateQRCodeUrl(string email, string secret)
    {
        var issuer = "MoneyTrackerApp";
        var otpAuthUrl = $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
        return $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(otpAuthUrl)}";
    }

    private bool VerifyTOTP(string secret, string code)
    {
        try
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = unixTimestamp / 30;

            for (int i = -1; i <= 1; i++)
            {
                var testCode = GenerateTOTP(secret, timeStep + i);
                if (testCode == code)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateTOTP(string secret, long timeStep)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timeBytes);

        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);

        var otp = binary % 1000000;
        return otp.ToString("D6");
    }
}
