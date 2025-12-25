using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoneyTrackerApp.Services;
using System.Security.Claims;
using System.Text.Json;

namespace MoneyTrackerApp.Pages.Analysis
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IFinancialAnalysisService _analysisService;

        public IndexModel(IFinancialAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        public CashflowForecastResult Forecast { get; set; } = new();
        public FinancialHealthScoreResult HealthScore { get; set; } = new();
        public string ForecastJson { get; set; } = "[]";
        public string RiskDateDisplay { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out long userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Load Data
            Forecast = await _analysisService.GetCashflowForecastAsync(userId, 60); // 60 days forecast
            HealthScore = await _analysisService.GetFinancialHealthScoreAsync(userId); // Health Score

            // Prepare Chart Data
            var chartData = Forecast.ForecastPoints.Select(p => new 
            {
                date = p.Date.ToString("dd/MM"),
                balance = p.PredictedBalance
            });
            ForecastJson = JsonSerializer.Serialize(chartData);

            if (Forecast.IsRiskZoneTouched && Forecast.RiskDate.HasValue)
            {
                RiskDateDisplay = Forecast.RiskDate.Value.ToString("dd/MM/yyyy");
            }

            return Page();
        }
    }
}
