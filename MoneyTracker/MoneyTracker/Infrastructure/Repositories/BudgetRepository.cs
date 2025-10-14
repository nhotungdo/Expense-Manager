using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Data;
using MoneyTracker.Models;

namespace MoneyTracker.Infrastructure.Repositories;

public class BudgetRepository : Repository<Budget>, IBudgetRepository
{
    public BudgetRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Budgets.CountAsync();
    }
}
