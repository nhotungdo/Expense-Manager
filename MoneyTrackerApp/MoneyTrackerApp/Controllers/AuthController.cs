using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoneyTrackerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace MoneyTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ExpenseManagerContext _db;
        private readonly IConfiguration _config;
        private readonly MoneyTrackerApp.Services.JwtTokenService _jwtService;
        private readonly MoneyTrackerApp.Services.IEmailService _emailService;
        private readonly MoneyTrackerApp.Services.ISessionService _sessionService;
        private readonly MoneyTrackerApp.Services.IMultiAccountService _multiAccountService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ExpenseManagerContext db, IConfiguration config, MoneyTrackerApp.Services.JwtTokenService jwtService, MoneyTrackerApp.Services.IEmailService emailService, MoneyTrackerApp.Services.ISessionService sessionService, MoneyTrackerApp.Services.IMultiAccountService multiAccountService, ILogger<AuthController> logger)
        {
            _db = db;
            _config = config;
            _jwtService = jwtService;
            _emailService = emailService;
            _sessionService = sessionService;
            _sessionService = sessionService;
            _multiAccountService = multiAccountService;
            _logger = logger;
        }

        public class RegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? FullName { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool RememberMe { get; set; }
        }

        public class TokenResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public bool OnboardingCompleted { get; set; }
            public string Role { get; set; } = string.Empty;
            public Guid SessionId { get; set; }
        }

        public class Verify2FARequest
        {
            public string Email { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public bool RememberMe { get; set; }
        }

        [HttpGet("google/start")]
        [AllowAnonymous]
        public IActionResult GoogleStart()
        {
            var props = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback")! };
            // Force account selection to allow adding multiple Google accounts
            props.Items["prompt"] = "select_account";
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var authResult = await HttpContext.AuthenticateAsync("External");
                if (!authResult.Succeeded || authResult.Principal == null) return Redirect("/Auth/Login?error=google_failed");

                var claims = authResult.Principal.Claims.ToList();
                var sub = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(sub)) return Redirect("/Auth/Login?error=google_no_sub");

                var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == sub || (email != null && u.Email == email));
                if (user == null)
                {
                    user = new User
                    {
                        GoogleId = sub,
                        Email = email,
                        UserName = email,
                        NormalizedEmail = email?.ToUpperInvariant(),
                        NormalizedUserName = email?.ToUpperInvariant(),
                        FullName = name,
                        EmailConfirmed = true,
                        Enabled = true,
                        Role = "User",
                        Language = "vi",
                        DefaultCurrency = "VND",
                        Timezone = "Asia/Ho_Chi_Minh",
                        Theme = "light"
                    };
                    _db.Users.Add(user);
                    await _db.SaveChangesAsync();
                }
                else if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = sub;
                    await _db.SaveChangesAsync();
                }

                // [Security] Assign Admin Rights if email matches
                if (email != null && email.Equals("nhotungdo89@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    if (user.Role != "Admin")
                    {
                        user.Role = "Admin";
                        await _db.SaveChangesAsync();
                    }
                }

                // [Audit] Log Login
                _db.AuditLogs.Add(new MoneyTrackerApp.Models.AuditLog
                {
                    UserId = user.Id,
                    Action = "Login",
                    Details = "Google Login Success",
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });
                await _db.SaveChangesAsync();

                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var session = await _sessionService.CreateSessionAsync(user.Id, userAgent, ipAddress);

                var pair = await _jwtService.IssueAsync(user, session.Id);

                Response.Cookies.Append("AccessToken", pair.access, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(60) // Google login default 60 mins (could ask user before but standard is session)
                });

                // [MultiAccount] Track Session
                _multiAccountService.AddSessionToCookie(session.Id);

                var url = $"/Auth/Login?accessToken={Uri.EscapeDataString(pair.access)}&refreshToken={Uri.EscapeDataString(pair.refresh)}&role={Uri.EscapeDataString(user.Role)}&sessionId={session.Id}";
                return Redirect(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google authentication failed");
                return Redirect($"/Auth/Login?error=google_unavailable&details={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<object>> Me()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(idStr, out var userId)) return Unauthorized();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized();
            return Ok(new { user.Id, user.Email, user.FullName, user.Role });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Register([FromBody] RegisterRequest req)
        {
            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains("@"))
                return BadRequest(new { message = "Email không hợp lệ" });

            if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
                return BadRequest(new { message = "Mật khẩu phải có ít nhất 6 ký tự" });

            if (string.IsNullOrWhiteSpace(req.FullName))
                return BadRequest(new { message = "Vui lòng nhập họ tên" });

            // 2. Check existence
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (existing != null) return BadRequest(new { message = "Email đã tồn tại" });

            // 3. Create User
            var user = new User
            {
                Email = req.Email,
                UserName = req.Email,
                NormalizedEmail = req.Email.ToUpperInvariant(),
                NormalizedUserName = req.Email.ToUpperInvariant(),
                FullName = req.FullName.Trim(),
                EmailConfirmed = true,
                Enabled = true,
                Role = "User",
                Language = "vi",
                DefaultCurrency = "VND",
                Timezone = "Asia/Ho_Chi_Minh",
                Theme = "light",
                EmailNotifications = true,
                // Fix: GoogleId is required and unique in DB. 
                // We generate a unique placeholder for local accounts.
                GoogleId = $"local_{Guid.NewGuid()}"
            };

            // 4. Hash Password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);

            try
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // [Security] Assign Admin Rights if email matches
                if (user.Email.Equals("nhotungdo89@gmail.com", StringComparison.OrdinalIgnoreCase))
                {
                    user.Role = "Admin";
                    await _db.SaveChangesAsync();
                }

                // [Notification] Send Welcome Email
                await _emailService.SendEmailToUserAsync(user.Id, "Chào mừng đến với MoneyTrackerApp!", 
                    $"<h3>Xin chào {user.FullName},</h3><p>Cảm ơn bạn đã đăng ký tài khoản tại MoneyTrackerApp. Bắt đầu quản lý tài chính của bạn ngay hôm nay!</p>");
            }
            catch (Exception ex)
            {
                // Return JSON error so frontend can display it instead of "Lỗi kết nối"
                return StatusCode(500, new { message = "Lỗi lưu dữ liệu: " + ex.Message });
            }

            // 5. Initialize Default Categories
            var categoryService = HttpContext.RequestServices.GetService<MoneyTrackerApp.Services.ICategoryService>();
            if (categoryService != null)
            {
                await categoryService.InitializeDefaultCategoriesAsync(user.Id);
            }

            // 6. Issue Tokens
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var session = await _sessionService.CreateSessionAsync(user.Id, userAgent, ipAddress);

            var pair = await _jwtService.IssueAsync(user, session.Id);

            // 6. Set Cookie (Important for auto-login consistency)
            Response.Cookies.Append("AccessToken", pair.access, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            // [MultiAccount] Track Session
            _multiAccountService.AddSessionToCookie(session.Id);

            var tokens = new TokenResponse
            {
                AccessToken = pair.access,
                RefreshToken = pair.refresh,
                OnboardingCompleted = user.OnboardingCompleted,
                Role = user.Role,
                SessionId = session.Id
            };
            return Ok(tokens);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return Unauthorized(new { message = "Sai email hoặc mật khẩu" });

            // 1. Check Lockout
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                return Unauthorized(new { message = $"Tài khoản bị khóa. Vui lòng thử lại sau {Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes)} phút." });
            }

            var ok = user.PasswordHash != null && BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
            if (!ok)
            {
                // Increment Failed Count
                user.AccessFailedCount++;
                
                // Lockout if >= 5 attempts
                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    user.AccessFailedCount = 0; // Reset after locking

                    await _db.SaveChangesAsync(); // Save lock status

                    // Send Lockout Email
                    await _emailService.SendEmailToUserAsync(user.Id, "Cảnh báo bảo mật: Tài khoản bị khóa tạm thời",
                        $"<h3>Tài khoản của bạn đã bị khóa tạm thời</h3><p>Hệ thống phát hiện 5 lần đăng nhập thất bại liên tiếp vào lúc {DateTime.UtcNow.ToString("HH:mm dd/MM/yyyy")}.</p><p>Tài khoản sẽ được mở lại sau 15 phút.</p>");

                    return Unauthorized(new { message = "Tài khoản đã bị khóa 15 phút do nhập sai quá nhiều lần." });
                }

                await _db.SaveChangesAsync();
                return Unauthorized(new { message = "Sai email hoặc mật khẩu" });
            }

            // Reset failed count on success
            if (user.AccessFailedCount > 0)
            {
                user.AccessFailedCount = 0;
            }

            // [Notification] Check New Device & Send Login Email
            var currentUa = Request.Headers["User-Agent"].ToString();
            var isNewDevice = !await _db.AuditLogs.AnyAsync(a => a.UserId == user.Id && a.Action == "Login" && a.UserAgent == currentUa);
            
            if (isNewDevice)
            {
                 await _emailService.SendEmailToUserAsync(user.Id, "Cảnh báo: Đăng nhập từ thiết bị mới",
                     $"<h3>Phát hiện đăng nhập từ thiết bị mới</h3><p>Thời gian: {DateTime.UtcNow.ToString("HH:mm dd/MM/yyyy UTC")}</p><p>Thiết bị: {currentUa}</p><p>Nếu không phải bạn, vui lòng đổi mật khẩu ngay.</p>");
            }
            else 
            {
                // Standard Login Notification (Requirement: "Gửi email khi đăng nhập thành công")
                 await _emailService.SendEmailToUserAsync(user.Id, "Thông báo đăng nhập thành công",
                     $"<h3>Đăng nhập thành công</h3><p>Thời gian: {DateTime.UtcNow.ToString("HH:mm dd/MM/yyyy UTC")}</p><p>Kiểm tra hoạt động tài khoản của bạn.</p>");
            }

            // [Security] Assign Admin Rights if email matches (Self-healing/Ensure)
            if (user.Email != null && user.Email.Equals("nhotungdo89@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                if (user.Role != "Admin")
                {
                    user.Role = "Admin";
                    await _db.SaveChangesAsync();
                }
            }

            // [Security] 2FA Check
            // Require 2FA if enabled OR if user is Admin
            if (user.TwoFactorEnabled || user.Role == "Admin")
            {
                // Generate 6-digit code
                var code = new Random().Next(100000, 999999).ToString();

                // Save or Update Token
                var existingToken = await _db.AspNetUserTokens
                    .FirstOrDefaultAsync(t => t.UserId == user.Id && t.LoginProvider == "Auth" && t.Name == "2FA");

                if (existingToken != null)
                {
                    existingToken.Value = code;
                }
                else
                {
                    _db.AspNetUserTokens.Add(new AspNetUserToken
                    {
                        UserId = user.Id,
                        LoginProvider = "Auth",
                        Name = "2FA",
                        Value = code
                    });
                }

                // Send Email
                _db.Emails.Add(new MoneyTrackerApp.Models.Email
                {
                    UserId = user.Id,
                    Subject = "Mã xác thực đăng nhập (2FA)",
                    Body = $"Mã xác thực của bạn là: {code}",
                    Status = "Queued",
                    CreatedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();

                // Return 2FA required status
                return Ok(new { message = "2fa_required", email = user.Email });
            }

            // If no 2FA, issue token immediately
            return await IssueTokenAndLog(user, "Login Success", req.RememberMe);
        }

        [HttpPost("verify-2fa")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Verify2FA([FromBody] Verify2FARequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return Unauthorized(new { message = "Người dùng không tồn tại" });

            var token = await _db.AspNetUserTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id && t.LoginProvider == "Auth" && t.Name == "2FA");

            if (token == null || token.Value != req.Code)
            {
                // [Audit] Log Failed
                _db.AuditLogs.Add(new MoneyTrackerApp.Models.AuditLog
                {
                    UserId = user.Id,
                    Action = "Login 2FA Failed",
                    Details = "Invalid Code",
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });
                await _db.SaveChangesAsync();
                return BadRequest(new { message = "Mã xác thực không đúng" });
            }

            // Consume token
            _db.AspNetUserTokens.Remove(token);
            await _db.SaveChangesAsync();

            return await IssueTokenAndLog(user, "Login 2FA Success", req.RememberMe);
        }

        private async Task<ActionResult<TokenResponse>> IssueTokenAndLog(User user, string auditAction, bool rememberMe = false)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var session = await _sessionService.CreateSessionAsync(user.Id, userAgent, ipAddress);

            var pair = await _jwtService.IssueAsync(user, session.Id);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(60)
            };
            
            Response.Cookies.Append("AccessToken", pair.access, cookieOptions);

            // [MultiAccount] Track Session
            _multiAccountService.AddSessionToCookie(session.Id);

            // [Audit] Log Success
            _db.AuditLogs.Add(new MoneyTrackerApp.Models.AuditLog
            {
                UserId = user.Id,
                Action = "Login",
                Details = auditAction + (rememberMe ? " (RememberMe)" : ""),
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            });
            await _db.SaveChangesAsync();

            var tokens = new TokenResponse
            {
                AccessToken = pair.access,
                RefreshToken = pair.refresh,
                OnboardingCompleted = user.OnboardingCompleted,
                Role = user.Role,
                SessionId = session.Id
            };
            return Ok(tokens);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Refresh([FromBody] TokenResponse body)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? token = null;
            try
            {
                token = handler.ReadJwtToken(body.RefreshToken);
            }
            catch
            {
                return Unauthorized();
            }

            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            if (!long.TryParse(userIdClaim, out var userId)) return Unauthorized();

            // Try to find in UserSessions first
            var session = await _db.UserSessions.FirstOrDefaultAsync(s => s.RefreshToken == body.RefreshToken);
            
            if (session != null)
            {
                if (!session.IsActive) return Unauthorized(new { message = "Session revoked" });

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return Unauthorized();

                // Rotate tokens in session
                var pair = await _jwtService.IssueAsync(user, session.Id);
                
                // Update Last Active
                await _sessionService.RefreshSessionActivityAsync(session.Id);

                var tokens = new TokenResponse
                {
                    AccessToken = pair.access,
                    RefreshToken = pair.refresh,
                    OnboardingCompleted = user.OnboardingCompleted,
                    Role = user.Role,
                    SessionId = session.Id
                };
                return Ok(tokens);
            }
            else
            {
                // Fallback to legacy
                var stored = await _db.AspNetUserTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.LoginProvider == "Auth" && t.Name == "RefreshToken" && t.Value == body.RefreshToken);
                if (stored == null) return Unauthorized();

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return Unauthorized();

                var pair = await _jwtService.IssueAsync(user); // No session ID passed, so it updates legacy token
                var tokens = new TokenResponse
                {
                    AccessToken = pair.access,
                    RefreshToken = pair.refresh,
                    OnboardingCompleted = user.OnboardingCompleted,
                    Role = user.Role
                };
                return Ok(tokens);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sidStr = User.FindFirst("sid")?.Value;
            
            if (long.TryParse(userIdStr, out var userId))
            {
                // Remove specific refresh token if we know the session, otherwise remove all? 
                // Better to remove only the current session's token.
                // But legacy code removes all "RefreshToken" tokens?
                // Line 529: returns IQueryable. Line 530: RemoveRange.
                // This removes ALL refresh tokens for the user device? No, just "RefreshToken" name.
                // Actually the current code removes ALL tokens named "RefreshToken" for that user.
                // That might kill other sessions on other devices if they share the same token name?
                // The DB schema shows AspNetUserTokens. 
                // Let's stick to cleaning up the current session from the cookie list.
                
                if (Guid.TryParse(sidStr, out var sid))
                {
                    _multiAccountService.RemoveSessionFromCookie(sid);
                    
                    // Also try to remove the session from UserSessions table to be clean
                    var dbSession = await _db.UserSessions.FindAsync(sid);
                    if (dbSession != null)
                    {
                        _db.UserSessions.Remove(dbSession);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            Response.Cookies.Delete("AccessToken");
            return Ok();
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return Ok();

            var resetToken = Guid.NewGuid().ToString("N");
            var store = await _db.AspNetUserTokens.FirstOrDefaultAsync(t => t.UserId == user.Id && t.LoginProvider == "Auth" && t.Name == "PasswordReset");
            if (store == null)
            {
                store = new AspNetUserToken { UserId = user.Id, LoginProvider = "Auth", Name = "PasswordReset" };
                _db.AspNetUserTokens.Add(store);
            }
            store.Value = resetToken;
            await _db.SaveChangesAsync();

            var subject = "Đặt lại mật khẩu";
            var content = $"Mã đặt lại mật khẩu của bạn: {resetToken}";
            _db.Emails.Add(new Email { UserId = user.Id, Subject = subject, Body = content, Status = "Queued" });
            await _db.SaveChangesAsync();

            return Ok();
        }

        public class ResetPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return BadRequest();
            var store = await _db.AspNetUserTokens.FirstOrDefaultAsync(t => t.UserId == user.Id && t.LoginProvider == "Auth" && t.Name == "PasswordReset" && t.Value == req.Token);
            if (store == null) return BadRequest(new { message = "Token không hợp lệ" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
            _db.AspNetUserTokens.Remove(store);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("send-otp")]
        [Authorize]
        public async Task<IActionResult> SendOtp()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!long.TryParse(userIdStr, out var userId)) return Unauthorized();

                var otpService = HttpContext.RequestServices.GetService<MoneyTrackerApp.Services.IOtpService>();
                if (otpService == null) return StatusCode(500, "OtpService not available");

                await otpService.GenerateAndSendOtpAsync(userId);
                
                // Audit Log
                _db.AuditLogs.Add(new MoneyTrackerApp.Models.AuditLog
                {
                    UserId = userId,
                    Action = "Generate OTP",
                    Details = "OTP requested for transaction/verification",
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });
                await _db.SaveChangesAsync();

                return Ok(new { message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("accounts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MoneyTrackerApp.DTOs.UserAccountSummaryDto>>> GetAvailableAccounts()
        {
            var sessionIds = _multiAccountService.GetSessionIdsFromCookie();
            if (!sessionIds.Any()) return Ok(new List<MoneyTrackerApp.DTOs.UserAccountSummaryDto>());

            var currentSidStr = User.FindFirst("sid")?.Value;
            Guid.TryParse(currentSidStr, out var currentSid);

            var sessions = await _db.UserSessions
                .Include(s => s.User)
                .Where(s => sessionIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            var result = sessions.Select(s => new MoneyTrackerApp.DTOs.UserAccountSummaryDto
            {
                UserId = s.UserId,
                Email = s.User.Email ?? "",
                FullName = s.User.FullName ?? "",
                SessionId = s.Id,
                IsActive = s.Id == currentSid,
                AvatarUrl = "/api/profile/avatar/" + s.UserId
            }).ToList();

            return Ok(result);
        }

        [HttpPost("switch-account/{sessionId}")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> SwitchAccount(Guid sessionId)
        {
            // 1. Validate the session is in our "Trusted" list (Cookie)
            var allowedSessions = _multiAccountService.GetSessionIdsFromCookie();
            if (!allowedSessions.Contains(sessionId))
            {
                return Forbidden("Session not accessible from this device.");
            }

            // 2. Fetch Session from DB
            var session = await _db.UserSessions.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null || !session.IsActive)
            {
                _multiAccountService.RemoveSessionFromCookie(sessionId);
                return Unauthorized("Session expired or invalid.");
            }

            // 3. Issue New Token
            var user = session.User;
            var pair = await _jwtService.IssueAsync(user, session.Id);

            // 4. Update Activity
            await _sessionService.RefreshSessionActivityAsync(session.Id);

            // 5. Update Cookies
            Response.Cookies.Append("AccessToken", pair.access, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });
            
            _multiAccountService.AddSessionToCookie(sessionId);

            return Ok(new TokenResponse
            {
                AccessToken = pair.access,
                RefreshToken = pair.refresh,
                OnboardingCompleted = user.OnboardingCompleted,
                Role = user.Role,
                SessionId = session.Id
            });
        }
        
        private ObjectResult Forbidden(string message) 
        {
            return StatusCode(403, new { message });
        }
    }
}