using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoneyTrackerApp.Pages;

[Authorize]
public class AiAdvisorModel : PageModel
{
    private readonly ILogger<AiAdvisorModel> _logger;

    public AiAdvisorModel(ILogger<AiAdvisorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("AI Advisor page accessed");
    }
}
