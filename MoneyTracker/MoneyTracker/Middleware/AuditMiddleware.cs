using MoneyTracker.Services;

namespace MoneyTracker.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuditService auditService)
        {
            var startTime = DateTime.UtcNow;

            // Skip audit for authentication-related paths to avoid redirect loops
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/login") || path.Contains("/logout") || path.Contains("/account") ||
                path.Contains("/api/auth") || path.Contains("/signin") || path.Contains("/signout"))
            {
                await _next(context);
                return;
            }

            // Log request
            await LogRequestAsync(context, auditService);

            await _next(context);

            // Log response
            await LogResponseAsync(context, auditService, startTime);
        }

        private async Task LogRequestAsync(HttpContext context, IAuditService auditService)
        {
            try
            {
                var userId = GetUserIdFromContext(context);
                if (userId.HasValue)
                {
                    var action = GetActionFromRequest(context);
                    var details = GetRequestDetails(context);

                    if (!string.IsNullOrEmpty(action))
                    {
                        await auditService.LogUserActionAsync(userId.Value, action, details);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log request audit");
            }
        }

        private async Task LogResponseAsync(HttpContext context, IAuditService auditService, DateTime startTime)
        {
            try
            {
                var userId = GetUserIdFromContext(context);
                if (userId.HasValue && context.Response.StatusCode >= 400)
                {
                    var duration = DateTime.UtcNow - startTime;
                    var details = $"Request failed with status {context.Response.StatusCode} after {duration.TotalMilliseconds}ms";

                    await auditService.LogUserActionAsync(userId.Value, "REQUEST_ERROR", details);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log response audit");
            }
        }

        private long? GetUserIdFromContext(HttpContext context)
        {
            try
            {
                var userIdClaim = context.User.FindFirst("sub")?.Value ??
                                 context.User.FindFirst("id")?.Value ??
                                 context.User.FindFirst("user_id")?.Value;

                if (long.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private string GetActionFromRequest(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Map HTTP methods and paths to actions
            if (path.Contains("/api/expenses"))
            {
                return method switch
                {
                    "GET" => "VIEW_EXPENSES",
                    "POST" => "CREATE_EXPENSE",
                    "PUT" => "UPDATE_EXPENSE",
                    "DELETE" => "DELETE_EXPENSE",
                    _ => "EXPENSE_ACTION"
                };
            }
            else if (path.Contains("/api/incomes"))
            {
                return method switch
                {
                    "GET" => "VIEW_INCOMES",
                    "POST" => "CREATE_INCOME",
                    "PUT" => "UPDATE_INCOME",
                    "DELETE" => "DELETE_INCOME",
                    _ => "INCOME_ACTION"
                };
            }
            else if (path.Contains("/api/categories"))
            {
                return method switch
                {
                    "GET" => "VIEW_CATEGORIES",
                    "POST" => "CREATE_CATEGORY",
                    "PUT" => "UPDATE_CATEGORY",
                    "DELETE" => "DELETE_CATEGORY",
                    _ => "CATEGORY_ACTION"
                };
            }
            else if (path.Contains("/api/dashboard"))
            {
                return "VIEW_DASHBOARD";
            }
            else if (path.Contains("/api/reports"))
            {
                return "VIEW_REPORTS";
            }
            else if (path.Contains("/api/ai-suggestions"))
            {
                return "VIEW_AI_SUGGESTIONS";
            }

            return method switch
            {
                "GET" => "VIEW_DATA",
                "POST" => "CREATE_DATA",
                "PUT" => "UPDATE_DATA",
                "DELETE" => "DELETE_DATA",
                _ => "UNKNOWN_ACTION"
            };
        }

        private string GetRequestDetails(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString;

            return $"{method} {path}{queryString}";
        }
    }
}
