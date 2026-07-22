using System.Linq.Expressions;
using Meridian.Domain.Common;

namespace Meridian.Application.Common.Interfaces;

/// <summary>
/// Repository pattern. Isolates the application layer from Entity Framework so
/// that services depend on an interface they can be tested against with an
/// in-memory fake, rather than on DbSet directly.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);

    /// <summary>
    /// Escape hatch for read paths that need projection or eager loading. Kept
    /// deliberately narrow: writes always go through the methods above.
    /// </summary>
    IQueryable<T> Query();
}
