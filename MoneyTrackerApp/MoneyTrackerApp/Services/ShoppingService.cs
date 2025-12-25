using Microsoft.EntityFrameworkCore;
using MoneyTrackerApp.Models;

namespace MoneyTrackerApp.Services;

public interface IShoppingService
{
    Task<List<ShoppingList>> GetListsAsync(long userId);
    Task<ShoppingList> GetListAsync(long listId, long userId);
    Task CreateListAsync(long userId, string name);
    Task DeleteListAsync(long listId, long userId);
    Task AddItemAsync(long listId, string name, decimal? price);
    Task ToggleItemAsync(long itemId);
    Task DeleteItemAsync(long itemId);
}

public class ShoppingService : IShoppingService
{
    private readonly ExpenseManagerContext _context;

    public ShoppingService(ExpenseManagerContext context)
    {
        _context = context;
    }

    public async Task<List<ShoppingList>> GetListsAsync(long userId)
    {
        return await _context.ShoppingLists
            .Include(l => l.ShoppingItems)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<ShoppingList> GetListAsync(long listId, long userId)
    {
        return await _context.ShoppingLists
            .Include(l => l.ShoppingItems)
            .FirstOrDefaultAsync(l => l.Id == listId && l.UserId == userId);
    }

    public async Task CreateListAsync(long userId, string name)
    {
        var list = new ShoppingList
        {
            UserId = userId,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
        _context.ShoppingLists.Add(list);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteListAsync(long listId, long userId)
    {
        var list = await _context.ShoppingLists.FirstOrDefaultAsync(l => l.Id == listId && l.UserId == userId);
        if (list != null)
        {
            _context.ShoppingLists.Remove(list);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddItemAsync(long listId, string name, decimal? price)
    {
        var item = new ShoppingItem
        {
            ShoppingListId = listId,
            Name = name,
            EstimatedPrice = price,
            IsPurchased = false
        };
        _context.ShoppingItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task ToggleItemAsync(long itemId)
    {
        var item = await _context.ShoppingItems.FindAsync(itemId);
        if (item != null)
        {
            item.IsPurchased = !item.IsPurchased;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteItemAsync(long itemId)
    {
        var item = await _context.ShoppingItems.FindAsync(itemId);
        if (item != null)
        {
            _context.ShoppingItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
