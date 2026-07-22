using Meridian.Domain.Common;

namespace Meridian.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern. Submitting an application writes to JobApplications,
/// ApplicationEvents, Notifications and AuditLogs; all four must commit together
/// or not at all, so the transaction boundary belongs here rather than in each
/// repository.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a transaction, saves, and commits.
    /// Rolls back if the operation throws.
    ///
    /// This is the method callers should reach for. The database is configured to
    /// retry transient failures, and a retry policy cannot resume a transaction it
    /// did not open, so the retry boundary has to sit outside the transaction
    /// rather than inside it. Wrapping that here means no caller has to know.
    /// The operation must be idempotent, because it can be executed more than once.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);

    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
