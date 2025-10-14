using MoneyTracker.Models;

namespace MoneyTracker.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ITransactionRepository Transactions { get; }
    ICategoryRepository Categories { get; }
    IBudgetRepository Budgets { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<Email> Emails { get; }
    IRepository<Report> Reports { get; }
    IRepository<SystemSettings> SystemSettings { get; }
    IRepository<AiSuggestion> AiSuggestions { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
