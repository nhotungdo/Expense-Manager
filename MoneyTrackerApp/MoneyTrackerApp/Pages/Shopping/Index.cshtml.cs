using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Shopping
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IShoppingService _shoppingService;

        public IndexModel(IShoppingService shoppingService)
        {
            _shoppingService = shoppingService;
        }

        public List<ShoppingList> Lists { get; set; } = new();

        [BindProperty]
        public string NewListName { get; set; }

        public async Task OnGetAsync()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Lists = await _shoppingService.GetListsAsync(userId);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (!string.IsNullOrWhiteSpace(NewListName))
            {
                await _shoppingService.CreateListAsync(userId, NewListName);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _shoppingService.DeleteListAsync(id, userId);
            return RedirectToPage();
        }
    }
}
