using Microsoft.EntityFrameworkCore.Storage;
using MoneyTracker.Core.Interfaces;
using MoneyTracker.Data;
using MoneyTracker.Models;

namespace MoneyTracker.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Transactions = new TransactionRepository(_context);
        Categories = new CategoryRepository(_context);
        Budgets = new BudgetRepository(_context);
        Notifications = new Repository<Notification>(_context);
        AuditLogs = new Repository<AuditLog>(_context);
        Emails = new Repository<Email>(_context);
        Reports = new Repository<Report>(_context);
        SystemSettings = new Repository<SystemSettings>(_context);
        AiSuggestions = new Repository<AiSuggestion>(_context);
    }

    public IUserRepository Users { get; }
    public ITransactionRepository Transactions { get; }
    public ICategoryRepository Categories { get; }
    public IBudgetRepository Budgets { get; }
    public IRepository<Notification> Notifications { get; }
    public IRepository<AuditLog> AuditLogs { get; }
    public IRepository<Email> Emails { get; }
    public IRepository<Report> Reports { get; }
    public IRepository<SystemSettings> SystemSettings { get; }
    public IRepository<AiSuggestion> AiSuggestions { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}
