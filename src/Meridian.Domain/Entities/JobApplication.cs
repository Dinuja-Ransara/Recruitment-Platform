using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

/// <summary>
/// A candidate's application to a posting. Named JobApplication rather than
/// Application to avoid colliding with the Meridian.Application namespace.
/// MatchScore and ScoreBreakdownJson are written by the matching engine and are
/// what the recruiter's explainability panel renders.
/// </summary>
public class JobApplication : BaseEntity
{
    public int JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;

    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public int? ResumeId { get; set; }
    public Resume? Resume { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    public string? CoverLetter { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public double? MatchScore { get; set; }
    public string? ScoreBreakdownJson { get; set; }
    public DateTime? ScoredAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token. A recruiter and a hiring manager can act on
    /// the same application at the same time, so the second write must fail loudly.
    /// </summary>
    public byte[]? RowVersion { get; set; }

    public ICollection<ApplicationEvent> Events { get; set; } = new List<ApplicationEvent>();
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}
