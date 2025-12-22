using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MoneyTrackerApp.Middleware
{
    public class OnboardingMiddleware
    {
        private readonly RequestDelegate _next;

        public OnboardingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var onboardingCompletedClaim = context.User.FindFirst("OnboardingCompleted")?.Value;
                
                // If claim is missing or explicitly false
                if (string.IsNullOrEmpty(onboardingCompletedClaim) || onboardingCompletedClaim.Equals("False", StringComparison.OrdinalIgnoreCase))
                {
                    var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

                    // Allow access to Onboarding pages, Logout, and API/Static resources
                    if (!path.StartsWith("/onboarding") && 
                        !path.StartsWith("/auth/logout") && 
                        !path.StartsWith("/api/") && 
                        !path.StartsWith("/css/") && 
                        !path.StartsWith("/js/") && 
                        !path.StartsWith("/lib/") && 
                        !path.StartsWith("/images/") &&
                        !path.Equals("/favicon.ico"))
                    {
                        context.Response.Redirect("/Onboarding/Index");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
