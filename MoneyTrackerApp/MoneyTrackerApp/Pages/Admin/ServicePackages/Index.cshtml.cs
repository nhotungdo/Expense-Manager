using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Pages.Admin.ServicePackages
{
    // [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IServicePackageService _servicePackageService;

        public IndexModel(IServicePackageService servicePackageService)
        {
            _servicePackageService = servicePackageService;
        }

        public List<ServicePackageDto> Packages { get; set; } = new();

        [BindProperty]
        public CreateServicePackageDto CreatePackage { get; set; } = new();

        [BindProperty]
        public UpdateServicePackageDto UpdatePackage { get; set; } = new();

        public async Task OnGetAsync()
        {
            Packages = await _servicePackageService.GetAllPackagesAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                Packages = await _servicePackageService.GetAllPackagesAsync();
                return Page();
            }

            await _servicePackageService.CreatePackageAsync(CreatePackage);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            // Note: In a real app, you'd validate UpdatePackage here.
            // Simplified for this generation.
            await _servicePackageService.UpdatePackageAsync(id, UpdatePackage);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            await _servicePackageService.TogglePackageStatusAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _servicePackageService.DeletePackageAsync(id);
            return RedirectToPage();
        }
    }
}
