using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services;

public interface IAssetService
{
    Task<List<Asset>> GetAssetsAsync(long userId);
    Task AddAssetAsync(Asset asset);
    Task DeleteAssetAsync(long id, long userId);
    Task CalculateDepreciationAsync(long userId);
    Task<decimal> GetTotalAssetValueAsync(long userId);
}

public class AssetService : IAssetService
{
    private readonly ExpenseManagerContext _context;

    public AssetService(ExpenseManagerContext context)
    {
        _context = context;
    }

    public async Task<List<Asset>> GetAssetsAsync(long userId)
    {
        return await _context.Assets
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.PurchaseDate)
            .ToListAsync();
    }

    public async Task AddAssetAsync(Asset asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAssetAsync(long id, long userId)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CalculateDepreciationAsync(long userId)
    {
        var assets = await _context.Assets.Where(a => a.UserId == userId).ToListAsync();
        foreach (var asset in assets)
        {
            if (asset.UsefulLifeMonths > 0)
            {
                var monthsPassed = ((DateTime.UtcNow.Year - asset.PurchaseDate.Year) * 12) + 
                                   DateTime.UtcNow.Month - asset.PurchaseDate.Month;
                
                if (monthsPassed < 0) monthsPassed = 0;

                var monthlyDepreciation = asset.InitialValue / asset.UsefulLifeMonths;
                var depreciatedValue = asset.InitialValue - (monthlyDepreciation * monthsPassed);

                if (depreciatedValue < 0) depreciatedValue = 0;
                
                asset.CurrentValue = Math.Round(depreciatedValue, 2);
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalAssetValueAsync(long userId)
    {
        return await _context.Assets
            .Where(a => a.UserId == userId)
            .SumAsync(a => a.CurrentValue);
    }
}
