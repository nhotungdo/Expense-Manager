using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTrackerApp.DTOs;
using MoneyTrackerApp.Services;

namespace MoneyTrackerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencyController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CurrencyResponseDto>>> GetCurrencies([FromQuery] bool includeInactive = false)
    {
        return Ok(await _currencyService.GetAllCurrenciesAsync(includeInactive));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CurrencyResponseDto>> CreateCurrency([FromBody] CreateCurrencyDto dto)
    {
        return Ok(await _currencyService.CreateCurrencyAsync(dto));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CurrencyResponseDto>> UpdateCurrency([FromBody] UpdateCurrencyDto dto)
    {
        return Ok(await _currencyService.UpdateCurrencyAsync(dto));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCurrency(int id)
    {
        var result = await _currencyService.DeleteCurrencyAsync(id);
        if (!result) return BadRequest("Could not delete currency");
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SetDefault(int id)
    {
        var result = await _currencyService.SetDefaultCurrencyAsync(id);
        if (!result) return BadRequest("Could not set default currency");
        return NoContent();
    }

    [HttpPost("convert")]
    public async Task<ActionResult<CurrencyConversionResponseDto>> Convert([FromBody] CurrencyConversionRequestDto dto)
    {
        return Ok(await _currencyService.ConvertAsync(dto));
    }

    [HttpPost("sync")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Sync()
    {
        await _currencyService.SyncRatesAsync();
        return NoContent();
    }
}
