using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace MoneyTrackerApp.Pages.Settings
{
    public class CurrenciesModel : PageModel
    {
        private readonly ILogger<CurrenciesModel> _logger;

        public CurrenciesModel(ILogger<CurrenciesModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
