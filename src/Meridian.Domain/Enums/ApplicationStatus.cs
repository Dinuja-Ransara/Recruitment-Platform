namespace Meridian.Domain.Enums;

/// <summary>
/// The recruitment pipeline. Transitions are validated in the application layer,
/// every change is recorded as an ApplicationEvent.
/// </summary>
public enum ApplicationStatus
{
    Submitted = 0,
    UnderReview = 1,
    Shortlisted = 2,
    InterviewScheduled = 3,
    Interviewed = 4,
    OfferExtended = 5,
    Hired = 6,
    Rejected = 7,
    Withdrawn = 8
}
