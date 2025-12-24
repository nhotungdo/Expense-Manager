using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services
{
    public interface ISessionService
    {
        Task<UserSession> CreateSessionAsync(long userId, string userAgent, string ipAddress);
        Task<UserSession?> GetSessionByIdAsync(Guid sessionId);
        Task<List<UserSession>> GetActiveSessionsAsync(long userId);
        Task TerminateSessionAsync(Guid sessionId);
        Task TerminateAllSessionsExceptAsync(long userId, Guid currentSessionId);
        Task RefreshSessionActivityAsync(Guid sessionId);
    }
}
