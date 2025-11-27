using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers;

/// <summary>
/// API Controller for Currency Exchange Rate management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrencyController> _logger;

    public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available currency exchange rates
    /// </summary>
    [HttpGet("rates")]
    public async Task<ActionResult<List<CurrencyRateDto>>> GetAllRates()
    {
        try
        {
            var rates = await _currencyService.GetAllRatesAsync();
            return Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting currency rates");
            return StatusCode(500, new { message = "An error occurred while retrieving currency rates" });
        }
    }

    /// <summary>
    /// Get exchange rate between two currencies
    /// </summary>
    [HttpGet("rates/{fromCurrency}/{toCurrency}")]
    public async Task<ActionResult<CurrencyRateDto>> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        try
        {
            var rate = await _currencyService.GetExchangeRateAsync(fromCurrency, toCurrency);
            
            if (rate == null)
                return NotFound(new { message = $"Exchange rate not found for {fromCurrency} to {toCurrency}" });

            return Ok(rate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate");
            return StatusCode(500, new { message = "An error occurred while retrieving the exchange rate" });
        }
    }

    /// <summary>
    /// Convert currency amount
    /// </summary>
    [HttpPost("convert")]
    public async Task<ActionResult<CurrencyConversionResultDto>> ConvertCurrency([FromBody] CurrencyConversionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _currencyService.ConvertCurrencyAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency");
            return StatusCode(500, new { message = "An error occurred while converting currency" });
        }
    }

    /// <summary>
    /// Update exchange rates (Admin only)
    /// </summary>
    [HttpPost("rates/update")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateExchangeRates()
    {
        try
        {
            await _currencyService.UpdateExchangeRatesAsync();
            return Ok(new { message = "Exchange rates updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exchange rates");
            return StatusCode(500, new { message = "An error occurred while updating exchange rates" });
        }
    }
}
