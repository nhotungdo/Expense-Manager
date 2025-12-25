using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MoneyTrackerApp.Models;
using System;
using System.Threading.Tasks;

namespace MoneyTrackerApp.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                 var sidClaim = context.User.FindFirst("sid");
                 if (sidClaim != null && Guid.TryParse(sidClaim.Value, out var sessionId))
                 {
                     var db = context.RequestServices.GetService<ExpenseManagerContext>();
                     if (db != null)
                     {
                         var session = await db.UserSessions.FindAsync(sessionId);
                         if (session == null || !session.IsActive)
                         {
                             // Clear the invalid cookie so the user isn't stuck in a loop
                             context.Response.Cookies.Delete("AccessToken");

                             if (context.Request.Path.StartsWithSegments("/api"))
                             {
                                 context.Response.StatusCode = 401;
                                 await context.Response.WriteAsJsonAsync(new { message = "Session revoked/expired" });
                             }
                             else
                             {
                                 // Redirect to login to re-authenticate
                                 context.Response.Redirect("/Auth/Login?reason=session_expired");
                             }
                             return;
                         }
                         
                         // Optional: Update LastActiveAt if needed, but might be performance heavy
                         // Keeping it simple: SessionService.RefreshSessionActivityAsync is called on Token Refresh.
                     }
                 }
            }
            await _next(context);
        }
    }
}
