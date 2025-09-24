using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyExpense.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string? Code { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Redirect to Google login for now; you can plug in OTP later.
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}


