using System.Collections.Concurrent;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Meridian.Infrastructure.Persistence.Repositories;

/// <summary>
/// Unit of Work implementation. Owns the DbContext for the lifetime of a request
/// and hands out repositories that all share it, so that a single SaveChangesAsync
/// commits every change made through any of them.
///
/// Repositories are cached per entity type: asking for Repository&lt;JobPosting&gt;
/// twice in one request returns the same instance rather than allocating a second.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly MeridianDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(MeridianDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
        => (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    /// <summary>
    /// Executes the operation as a retriable transactional unit.
    ///
    /// SqlServerRetryingExecutionStrategy refuses to take part in a transaction
    /// that was opened outside its control, because on a retry it would have no
    /// way to roll the earlier attempt back. Asking the provider for its own
    /// execution strategy and opening the transaction inside it resolves that:
    /// the strategy owns the retry boundary, and everything within one attempt
    /// commits or rolls back together.
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await operation(ct);
                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A transaction is already in progress on this unit of work.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("Commit was called without an active transaction.");
        }

        try
        {
            await _context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _transaction?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
