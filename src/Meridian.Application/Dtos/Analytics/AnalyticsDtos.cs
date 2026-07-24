using Meridian.Domain.Enums;

namespace Meridian.Application.Dtos.Analytics;

/// <summary>
/// Recruitment analytics for one organisation.
///
/// Every figure is derived from the application pipeline rather than stored, so
/// the dashboard cannot drift out of step with the applications it describes.
/// </summary>
public record RecruitmentAnalyticsDto
{
    public int PublishedPostings { get; init; }
    public int DraftPostings { get; init; }
    public int TotalApplications { get; init; }
    public int ActiveApplications { get; init; }

    /// <summary>Mean match score across every scored application, 0 to 100.</summary>
    public double AverageMatchScore { get; init; }

    /// <summary>
    /// Share of applications rejected specifically for a missing mandatory skill.
    /// A high value suggests postings are advertising requirements the market
    /// cannot meet, which is actionable in a way a raw rejection count is not.
    /// </summary>
    public double MandatoryGapRejectionRate { get; init; }

    public IReadOnlyList<FunnelStageDto> Funnel { get; init; } = Array.Empty<FunnelStageDto>();
    public IReadOnlyList<PostingPerformanceDto> PostingPerformance { get; init; } = Array.Empty<PostingPerformanceDto>();
    public IReadOnlyList<LocationBreakdownDto> ByLocation { get; init; } = Array.Empty<LocationBreakdownDto>();
    public IReadOnlyList<SkillDemandDto> MostRequestedSkills { get; init; } = Array.Empty<SkillDemandDto>();
}

/// <summary>
/// One stage of the hiring funnel. Count is the number of applications that
/// reached this stage or moved beyond it, so the sequence decreases monotonically
/// and reads as a funnel rather than a snapshot of current states.
/// </summary>
public record FunnelStageDto(ApplicationStatus Status, string Label, int Count, double PercentageOfTotal);

public record PostingPerformanceDto
{
    public int JobPostingId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public int Applications { get; init; }
    public int Shortlisted { get; init; }
    public double AverageScore { get; init; }
    public double TopScore { get; init; }
}

public record LocationBreakdownDto(string Location, int Postings, int Applications);

public record SkillDemandDto(string Skill, int PostingsRequiring, int MandatoryIn);
