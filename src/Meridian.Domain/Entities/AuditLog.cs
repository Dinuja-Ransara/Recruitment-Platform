using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// Append-only record of security-relevant actions, required by the brief's data
/// privacy and audit logging requirement. Never updated or deleted.
/// </summary>
public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
