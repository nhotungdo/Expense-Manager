using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Models;

namespace MoneyTracker.Pages.Account;

public class ExternalLoginModel : PageModel
{
    private readonly ExpenseManagerContext _db;
    public ExternalLoginModel(ExpenseManagerContext db) { _db = db; }

    [BindProperty(SupportsGet = true)]
    public string Provider { get; set; } = "Google";

    public IActionResult OnGet(string provider)
    {
        Provider = provider;
        return Page();
    }

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page("/Account/ExternalLoginCallback", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }
}


