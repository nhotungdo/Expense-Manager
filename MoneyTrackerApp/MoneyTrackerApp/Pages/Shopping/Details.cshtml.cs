using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Shopping
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IShoppingService _shoppingService;

        public DetailsModel(IShoppingService shoppingService)
        {
            _shoppingService = shoppingService;
        }

        public ShoppingList ShoppingList { get; set; }

        [BindProperty]
        public string ItemName { get; set; }
        
        [BindProperty]
        public decimal? ItemPrice { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ShoppingList = await _shoppingService.GetListAsync(id, userId);
            
            if (ShoppingList == null) return RedirectToPage("./Index");
            
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync(long id)
        {
             if (!string.IsNullOrWhiteSpace(ItemName))
             {
                 await _shoppingService.AddItemAsync(id, ItemName, ItemPrice);
             }
             return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostToggleAsync(long id, long itemId)
        {
            await _shoppingService.ToggleItemAsync(itemId);
            return RedirectToPage(new { id });
        }

         public async Task<IActionResult> OnPostDeleteAsync(long id, long itemId)
        {
            await _shoppingService.DeleteItemAsync(itemId);
            return RedirectToPage(new { id });
        }
    }
}
