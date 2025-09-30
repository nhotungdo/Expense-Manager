using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTracker.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GoogleClientId { get; set; } = string.Empty;

        public void OnGet()
        {
            GoogleClientId = _configuration["GoogleAuth:ClientId"] ?? "";
        }
    }
}
