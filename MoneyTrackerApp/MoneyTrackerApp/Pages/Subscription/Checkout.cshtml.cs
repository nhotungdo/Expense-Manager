using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Subscription;

public class CheckoutModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int PackageId { get; set; } = 2;

    public long UserId { get; private set; }
    public string UserEmail { get; private set; } = string.Empty;

    private readonly MoneyTrackerApp.Models.ExpenseManagerContext _context;

    public CheckoutModel(MoneyTrackerApp.Models.ExpenseManagerContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        if (PackageId <= 0) PackageId = 2;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(claim, out var id))
        {
            UserId = id;
            var user = _context.Users.Find(UserId);
            if (user != null)
            {
                UserEmail = user.Email ?? "";
            }
        }
        else
        {
            UserId = 0; // guest/placeholder
        }
    }
}





