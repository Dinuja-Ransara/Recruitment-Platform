using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

/// <summary>
/// A message queued for a user. The Channel decides which provider the Abstract
/// Factory supplies to deliver it.
/// </summary>
public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime? SentAt { get; set; }
    public string? RelatedEntityName { get; set; }
    public int? RelatedEntityId { get; set; }
}
