namespace Meridian.Domain.Common;

/// <summary>
/// Base type for every persisted entity. Identity and audit timestamps are
/// centralised here so that no entity re-declares them.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
