using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Models;
using MoneyTrackerApp.Services;
using System.Security.Claims;

namespace MoneyTrackerApp.Pages.Assets
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IAssetService _assetService;

        public IndexModel(IAssetService assetService)
        {
            _assetService = assetService;
        }

        public List<Asset> Assets { get; set; } = new();

        [BindProperty]
        public AssetInput Input { get; set; } = new();

        public class AssetInput
        {
            public string Name { get; set; }
            public decimal InitialValue { get; set; }
            public int UsefulLifeMonths { get; set; }
            public DateTime PurchaseDate { get; set; } = DateTime.Today;
        }

        public async Task OnGetAsync()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Trigger update first
            await _assetService.CalculateDepreciationAsync(userId);
            Assets = await _assetService.GetAssetsAsync(userId);
        }

        public async Task<IActionResult> OnPostAsync()
        {
             var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
             if (ModelState.IsValid)
             {
                 var asset = new Asset
                 {
                     UserId = userId,
                     Name = Input.Name,
                     InitialValue = Input.InitialValue,
                     CurrentValue = Input.InitialValue, // Start full
                     UsefulLifeMonths = Input.UsefulLifeMonths,
                     PurchaseDate = Input.PurchaseDate,
                     CreatedAt = DateTime.UtcNow
                 };
                 await _assetService.AddAssetAsync(asset);
             }
             return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
             var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
             await _assetService.DeleteAssetAsync(id, userId);
             return RedirectToPage();
        }
    }
}
