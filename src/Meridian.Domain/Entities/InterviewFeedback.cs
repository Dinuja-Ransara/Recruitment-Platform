using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

public class InterviewFeedback : BaseEntity
{
    public int InterviewId { get; set; }
    public Interview Interview { get; set; } = null!;

    public int SubmittedByUserId { get; set; }
    public User SubmittedBy { get; set; } = null!;

    public string Strengths { get; set; } = string.Empty;
    public string Concerns { get; set; } = string.Empty;
    public HiringRecommendation Recommendation { get; set; } = HiringRecommendation.Neutral;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
