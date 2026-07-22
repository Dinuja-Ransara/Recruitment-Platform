using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

/// <summary>
/// A scheduled interview. CalendarUid is the UID written into the generated .ics
/// invitation so that a later reschedule updates the same calendar entry rather
/// than creating a second one.
/// </summary>
public class Interview : BaseEntity
{
    public int JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public int InterviewerUserId { get; set; }
    public User Interviewer { get; set; } = null!;

    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public InterviewMode Mode { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public string? MeetingUrl { get; set; }
    public string? LocationNote { get; set; }
    public string CalendarUid { get; set; } = Guid.NewGuid().ToString();

    public InterviewFeedback? Feedback { get; set; }
}
