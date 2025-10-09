using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTracker.Models;
using System.Security.Claims;

namespace MoneyTracker.Pages.Identity.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public string Provider { get; set; } = string.Empty;

        [BindProperty]
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? provider = null, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(provider))
            {
                return BadRequest("Provider is required");
            }

            Provider = provider;
            ReturnUrl = returnUrl ?? Url.Content("~/");

            // Challenge the external login provider
            var redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl = ReturnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                _logger.LogError("External login error: {Error}", remoteError);
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("External login info not found");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var pictureUrl = info.Principal.FindFirstValue("picture");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email not found in external login info");
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    // Add external login to existing user
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        _logger.LogInformation("User logged in with {Provider} provider", info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    // Create new user
                    var user = new User
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        FullName = name,
                        PictureUrl = pictureUrl,
                        GoogleId = info.ProviderKey,
                        Role = "USER",
                        Enabled = true,
                        CreatedAt = DateTime.UtcNow,
                        Language = "vi",
                        DefaultCurrency = "VND",
                        Timezone = "Asia/Ho_Chi_Minh",
                        Theme = "light",
                        EmailNotifications = true,
                        PushNotifications = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        var addLoginResult = await _userManager.AddLoginAsync(user, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation("User created and logged in with {Provider} provider", info.LoginProvider);
                            return LocalRedirect(returnUrl);
                        }
                    }

                    foreach (var error in createResult.Errors)
                    {
                        _logger.LogError("Error creating user: {Error}", error.Description);
                    }
                }
            }

            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
    }
}
