using MoneyTrackerApp.Models;
using MoneyTrackerApp.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MoneyTrackerApp.Services;

public interface ICurrencyService
{
    Task<List<CurrencyResponseDto>> GetAllCurrenciesAsync(bool includeInactive = false);
    Task<CurrencyResponseDto?> GetCurrencyByCodeAsync(string code);
    Task<CurrencyResponseDto> CreateCurrencyAsync(CreateCurrencyDto dto);
    Task<CurrencyResponseDto> UpdateCurrencyAsync(UpdateCurrencyDto dto);
    Task<bool> DeleteCurrencyAsync(int id);
    Task<bool> SetDefaultCurrencyAsync(int id);
    Task<CurrencyConversionResponseDto> ConvertAsync(CurrencyConversionRequestDto dto);
    Task SyncRatesAsync();
    Task<decimal> GetRateAsync(string fromCode, string toCode);
    Task SeedAsync();
}

public class CurrencyService : ICurrencyService
{
    private readonly ExpenseManagerContext _context;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyService> _logger;
    private const string CACHE_KEY_PREFIX = "currency_rate_";

    public CurrencyService(
        ExpenseManagerContext context, 
        IMemoryCache cache, 
        HttpClient httpClient,
        ILogger<CurrencyService> logger)
    {
        _context = context;
        _cache = cache;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CurrencyResponseDto>> GetAllCurrenciesAsync(bool includeInactive = false)
    {
        var query = _context.Currencies.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        var currencies = await query.OrderBy(c => c.Code).ToListAsync();
        return currencies.Select(MapToResponseDto).ToList();
    }

    public async Task<CurrencyResponseDto?> GetCurrencyByCodeAsync(string code)
    {
        var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == code);
        return currency != null ? MapToResponseDto(currency) : null;
    }

    public async Task<CurrencyResponseDto> CreateCurrencyAsync(CreateCurrencyDto dto)
    {
        var currency = new Currency
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            Symbol = dto.Symbol,
            ExchangeRate = dto.ExchangeRate,
            Country = dto.Country,
            FlagUrl = dto.FlagUrl,
            IsDefault = dto.IsDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        if (dto.IsDefault)
        {
            await ClearDefaultsAsync();
        }

        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync();
        return MapToResponseDto(currency);
    }

    public async Task<CurrencyResponseDto> UpdateCurrencyAsync(UpdateCurrencyDto dto)
    {
        var currency = await _context.Currencies.FindAsync(dto.Id);
        if (currency == null) throw new Exception("Currency not found");

        if (dto.Name != null) currency.Name = dto.Name;
        if (dto.Symbol != null) currency.Symbol = dto.Symbol;
        if (dto.ExchangeRate.HasValue) currency.ExchangeRate = dto.ExchangeRate.Value;
        if (dto.Country != null) currency.Country = dto.Country;
        if (dto.FlagUrl != null) currency.FlagUrl = dto.FlagUrl;
        if (dto.IsActive.HasValue) currency.IsActive = dto.IsActive.Value;
        
        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            await ClearDefaultsAsync();
            currency.IsDefault = true;
        }

        currency.UpdatedAt = DateTime.UtcNow;
        _context.Currencies.Update(currency);
        await _context.SaveChangesAsync();
        return MapToResponseDto(currency);
    }

    public async Task SeedAsync()
    {
        if (await _context.Currencies.AnyAsync()) return;

        var currencies = new List<Currency>
        {
            new Currency { Code = "VND", Name = "Vietnamese Dong", Symbol = "₫", Country = "Vietnam", FlagUrl = "vn", ExchangeRate = 25680m, IsDefault = true, IsActive = true },
            new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", Country = "USA", FlagUrl = "us", ExchangeRate = 1.0m, IsDefault = false, IsActive = true },
            new Currency { Code = "EUR", Name = "Euro", Symbol = "€", Country = "European Union", FlagUrl = "eu", ExchangeRate = 0.91m, IsDefault = false, IsActive = true },
            new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", Country = "UK", FlagUrl = "gb", ExchangeRate = 0.76m, IsDefault = false, IsActive = true },
            new Currency { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", Country = "Japan", FlagUrl = "jp", ExchangeRate = 144.2m, IsDefault = false, IsActive = true },
            new Currency { Code = "KRW", Name = "South Korean Won", Symbol = "₩", Country = "South Korea", FlagUrl = "kr", ExchangeRate = 1350m, IsDefault = false, IsActive = true },
            new Currency { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", Country = "China", FlagUrl = "cn", ExchangeRate = 7.17m, IsDefault = false, IsActive = true },
            new Currency { Code = "SGD", Name = "Singapore Dollar", Symbol = "S$", Country = "Singapore", FlagUrl = "sg", ExchangeRate = 1.32m, IsDefault = false, IsActive = true },
            new Currency { Code = "THB", Name = "Thai Baht", Symbol = "฿", Country = "Thailand", FlagUrl = "th", ExchangeRate = 35.5m, IsDefault = false, IsActive = true },
            new Currency { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", Country = "Australia", FlagUrl = "au", ExchangeRate = 1.49m, IsDefault = false, IsActive = true },
            new Currency { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", Country = "Canada", FlagUrl = "ca", ExchangeRate = 1.34m, IsDefault = false, IsActive = true },
            new Currency { Code = "BTC", Name = "Bitcoin", Symbol = "₿", Country = "Crypto", FlagUrl = "btc", ExchangeRate = 0.000015m, IsDefault = false, IsActive = true },
            new Currency { Code = "ETH", Name = "Ethereum", Symbol = "Ξ", Country = "Crypto", FlagUrl = "eth", ExchangeRate = 0.00035m, IsDefault = false, IsActive = true },
            new Currency { Code = "USDT", Name = "Tether", Symbol = "₮", Country = "Crypto", FlagUrl = "usdt", ExchangeRate = 1.0m, IsDefault = false, IsActive = true }
        };

        _context.Currencies.AddRange(currencies);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCurrencyAsync(int id)
    {
        var currency = await _context.Currencies.FindAsync(id);
        if (currency == null || currency.IsDefault) return false;

        _context.Currencies.Remove(currency);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetDefaultCurrencyAsync(int id)
    {
        var currency = await _context.Currencies.FindAsync(id);
        if (currency == null) return false;

        await ClearDefaultsAsync();
        currency.IsDefault = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CurrencyConversionResponseDto> ConvertAsync(CurrencyConversionRequestDto dto)
    {
        var rate = await GetRateAsync(dto.FromCode, dto.ToCode);
        return new CurrencyConversionResponseDto
        {
            FromCode = dto.FromCode,
            ToCode = dto.ToCode,
            OriginalAmount = dto.Amount,
            ConvertedAmount = dto.Amount * rate,
            Rate = rate,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<decimal> GetRateAsync(string fromCode, string toCode)
    {
        if (fromCode == toCode) return 1.0m;

        var cacheKey = $"{fromCode}_{toCode}";
        if (_cache.TryGetValue(cacheKey, out decimal cachedRate))
        {
            return cachedRate;
        }

        var currencies = await _context.Currencies
            .Where(c => c.Code == fromCode || c.Code == toCode)
            .ToListAsync();

        var from = currencies.FirstOrDefault(c => c.Code == fromCode);
        var to = currencies.FirstOrDefault(c => c.Code == toCode);

        if (from == null || to == null)
        {
            // Fallback to existing CurrencyRates table if possible, or throw
            throw new Exception($"Currency rates not found for {fromCode} or {toCode}");
        }

        // Formula: (Amount / FromRateRelativeToUSD) * ToRateRelativeToUSD
        // So cross rate is ToRate / FromRate
        var rate = to.ExchangeRate / from.ExchangeRate;

        _cache.Set(cacheKey, rate, TimeSpan.FromMinutes(30));
        return rate;
    }

    public async Task SyncRatesAsync()
    {
        try
        {
            await SeedAsync();
            _logger.LogInformation("Starting currency rate sync...");
            
            // Using ExchangeRate-API (Free tier allows 1,500 requests/month)
            // Replace YOUR-API-KEY with actual key if available, otherwise use free endpoint if possible
            // Most free endpoints use USD as base.
            var response = await _httpClient.GetAsync("https://open.er-api.com/v6/latest/USD");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch rates from external API. Using cached values.");
                return;
            }

            var data = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>();
            if (data?.rates == null) return;

            var currencies = await _context.Currencies.ToListAsync();
            foreach (var currency in currencies)
            {
                if (data.rates.TryGetValue(currency.Code, out decimal newRate))
                {
                    currency.ExchangeRate = newRate;
                    currency.LastUpdated = DateTime.UtcNow;
                    currency.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Currency rate sync completed successfully.");
            
            // Clear cache
            foreach (var from in currencies)
            {
                foreach (var to in currencies)
                {
                    _cache.Remove($"{from.Code}_{to.Code}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing currency rates");
        }
    }

    private async Task ClearDefaultsAsync()
    {
        var defaults = await _context.Currencies.Where(c => c.IsDefault).ToListAsync();
        foreach (var c in defaults)
        {
            c.IsDefault = false;
        }
    }

    private CurrencyResponseDto MapToResponseDto(Currency c)
    {
        return new CurrencyResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            Symbol = c.Symbol,
            ExchangeRate = c.ExchangeRate,
            Country = c.Country,
            FlagUrl = c.FlagUrl,
            IsDefault = c.IsDefault,
            IsActive = c.IsActive,
            LastUpdated = c.LastUpdated,
            TimeAgo = GetTimeAgo(c.LastUpdated)
        };
    }

    private string GetTimeAgo(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return "Never";
        var span = DateTime.UtcNow - dateTime.Value;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        return dateTime.Value.ToString("dd/MM/yyyy");
    }
}

public class ExchangeRateApiResponse
{
    public string result { get; set; }
    public Dictionary<string, decimal> rates { get; set; }
    public string base_code { get; set; }
}
