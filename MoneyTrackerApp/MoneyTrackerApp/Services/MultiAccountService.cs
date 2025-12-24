using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace MoneyTrackerApp.Services
{
    public interface IMultiAccountService
    {
        void AddSessionToCookie(Guid sessionId);
        void RemoveSessionFromCookie(Guid sessionId);
        List<Guid> GetSessionIdsFromCookie();
        void ClearCookie();
    }

    public class MultiAccountService : IMultiAccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _protector;
        private const string CookieName = "MoneyTracker.Accounts";

        public MultiAccountService(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider provider)
        {
            _httpContextAccessor = httpContextAccessor;
            _protector = provider.CreateProtector("MoneyTrackerApp.MultiAccountProtection");
        }

        public void AddSessionToCookie(Guid sessionId)
        {
            var sessions = GetSessionIdsFromCookie();
            if (!sessions.Contains(sessionId))
            {
                sessions.Add(sessionId);
                SaveCookie(sessions);
            }
        }

        public void RemoveSessionFromCookie(Guid sessionId)
        {
            var sessions = GetSessionIdsFromCookie();
            if (sessions.Remove(sessionId))
            {
                SaveCookie(sessions);
            }
        }

        public List<Guid> GetSessionIdsFromCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return new List<Guid>();

            if (context.Request.Cookies.TryGetValue(CookieName, out var protectedData))
            {
                try
                {
                    var json = _protector.Unprotect(protectedData);
                    return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
                }
                catch
                {
                    // Invalid cookie or decryption failed
                    return new List<Guid>();
                }
            }
            return new List<Guid>();
        }

        public void ClearCookie()
        {
            var context = _httpContextAccessor.HttpContext;
            context?.Response.Cookies.Delete(CookieName);
        }

        private void SaveCookie(List<Guid> sessions)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var json = JsonSerializer.Serialize(sessions);
            var protectedData = _protector.Protect(json);

            context.Response.Cookies.Append(CookieName, protectedData, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // localhost supports Secure in most modern browsers or dev certs
                SameSite = SameSiteMode.Lax, // Lax allows top-level nav
                Expires = DateTime.UtcNow.AddDays(90) // Long lived list of accounts
            });
        }
    }
}
