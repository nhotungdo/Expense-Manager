using MoneyTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyTrackerApp.Services;

public class SessionService : ISessionService
{
    private readonly ExpenseManagerContext _context;

    public SessionService(ExpenseManagerContext context)
    {
        _context = context;
    }

    public async Task<UserSession> CreateSessionAsync(long userId, string userAgent, string ipAddress)
    {
        var session = new UserSession
        {
            UserId = userId,
            IpAddress = ipAddress,
            DeviceName = ParseDeviceName(userAgent),
            Browser = ParseBrowser(userAgent),
            OperatingSystem = ParseOS(userAgent),
            Location = "Unknown", // Placeholder for GeoIP integration
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30), // Configurable
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<UserSession?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _context.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<List<UserSession>> GetActiveSessionsAsync(long userId)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastActiveAt)
            .ToListAsync();
    }

    public async Task TerminateSessionAsync(Guid sessionId)
    {
        var session = await _context.UserSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task TerminateAllSessionsExceptAsync(long userId, Guid currentSessionId)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive && s.Id != currentSessionId)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
        }
        await _context.SaveChangesAsync();
    }

    public async Task RefreshSessionActivityAsync(Guid sessionId)
    {
        var session = await _context.UserSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string ParseDeviceName(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown Device";
        if (userAgent.Contains("Android")) return "Android Device";
        if (userAgent.Contains("iPhone")) return "iPhone";
        if (userAgent.Contains("iPad")) return "iPad";
        if (userAgent.Contains("Macintosh") || userAgent.Contains("Mac OS")) return "Mac";
        if (userAgent.Contains("Windows")) return "Windows PC";
        return "Unknown Device";
    }

    private string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown Browser";
        if (userAgent.Contains("Edg")) return "Edge";
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg")) return "Chrome";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
        return "Unknown Browser";
    }

    private string ParseOS(string userAgent)
    {
         if (string.IsNullOrEmpty(userAgent)) return "Unknown OS";
         if (userAgent.Contains("Windows")) return "Windows";
         if (userAgent.Contains("Mac OS")) return "macOS";
         if (userAgent.Contains("Linux")) return "Linux";
         if (userAgent.Contains("Android")) return "Android";
         if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
         return "Unknown OS";
    }
}
