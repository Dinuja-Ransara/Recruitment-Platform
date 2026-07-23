using System.ComponentModel.DataAnnotations;
using Meridian.Domain.Enums;

namespace Meridian.Application.Dtos.Applications;

public record ApplyRequest
{
    [Range(1, int.MaxValue)]
    public int JobPostingId { get; init; }

    /// <summary>Optional. Falls back to the candidate's primary resume.</summary>
    public int? ResumeId { get; init; }

    [MaxLength(4000)]
    public string? CoverLetter { get; init; }
}

public record ApplicationSummaryDto
{
    public int Id { get; init; }
    public int JobPostingId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public DateTime SubmittedAt { get; init; }
    public double? MatchScore { get; init; }
    public string? MatchSummary { get; init; }
}

/// <summary>An applicant as the recruiter sees them, with the score breakdown.</summary>
public record ApplicantDto
{
    public int ApplicationId { get; init; }
    public int CandidateProfileId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Headline { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public decimal YearsOfExperience { get; init; }
    public EducationLevel HighestEducation { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime SubmittedAt { get; init; }

    public double? MatchScore { get; init; }
    public MatchExplanationDto? Explanation { get; init; }
}

/// <summary>
/// The score breakdown, carried to the client so the recruiter can see why a
/// candidate ranked where they did rather than being handed a bare number.
/// </summary>
public record MatchExplanationDto
{
    public double Score { get; init; }
    public string Strategy { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public bool HasMandatoryGap { get; init; }
    public IReadOnlyList<ScoreFactorDto> Factors { get; init; } = Array.Empty<ScoreFactorDto>();
    public IReadOnlyList<SkillMatchDto> MatchedSkills { get; init; } = Array.Empty<SkillMatchDto>();
    public IReadOnlyList<SkillGapDto> MissingSkills { get; init; } = Array.Empty<SkillGapDto>();
}

public record ScoreFactorDto(string Name, double Value, double Weight, double Contribution, string Detail);

public record SkillMatchDto(string Skill, bool IsMandatory, decimal CandidateYears, decimal RequiredYears, bool MeetsExperienceBar);

public record SkillGapDto(string Skill, bool IsMandatory, decimal RequiredYears);

public record ApplicationDetailDto : ApplicationSummaryDto
{
    public string? CoverLetter { get; init; }
    public MatchExplanationDto? Explanation { get; init; }
    public IReadOnlyList<ApplicationEventDto> Timeline { get; init; } = Array.Empty<ApplicationEventDto>();
}

public record ApplicationEventDto(
    ApplicationStatus? FromStatus, ApplicationStatus ToStatus, string? Note, string? ActorName, DateTime OccurredAt);

public record ChangeStatusRequest
{
    public ApplicationStatus Status { get; init; }

    [MaxLength(1000)]
    public string? Note { get; init; }
}

/// <summary>A recommended posting for a candidate, with its reasoning.</summary>
public record JobRecommendationDto
{
    public int JobPostingId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public WorkMode WorkMode { get; init; }
    public bool AlreadyApplied { get; init; }
    public MatchExplanationDto Explanation { get; init; } = new();
}
