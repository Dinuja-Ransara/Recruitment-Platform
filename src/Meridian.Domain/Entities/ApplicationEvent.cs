using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

/// <summary>
/// One immutable step in an application's history. Written by the domain event
/// dispatcher on every status transition, and rendered as the candidate's timeline.
/// </summary>
public class ApplicationEvent : BaseEntity
{
    public int JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public ApplicationStatus? FromStatus { get; set; }
    public ApplicationStatus ToStatus { get; set; }
    public string? Note { get; set; }

    public int? ActorUserId { get; set; }
    public User? ActorUser { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
