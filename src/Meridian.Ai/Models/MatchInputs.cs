using Meridian.Domain.Enums;

namespace Meridian.Ai.Models;

/// <summary>
/// What the engine needs to know about a posting. Deliberately a plain record
/// rather than the JobPosting entity, so the engine has no dependency on Entity
/// Framework, no navigation properties to lazy-load, and can be unit tested with
/// literals instead of a database.
/// </summary>
public record JobProfile
{
    public int JobId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public WorkMode WorkMode { get; init; }
    public SeniorityLevel Seniority { get; init; }
    public decimal MinYearsExperience { get; init; }
    public EducationLevel RequiredEducation { get; init; }
    public IReadOnlyList<SkillRequirement> Skills { get; init; } = Array.Empty<SkillRequirement>();
}

public record SkillRequirement(string Skill, bool IsMandatory, int Weight, decimal MinYears);

/// <summary>What the engine needs to know about a candidate.</summary>
public record CandidateProfileSnapshot
{
    public int CandidateId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Headline { get; init; } = string.Empty;
    public string ResumeText { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public bool IsOpenToRemote { get; init; }
    public decimal YearsOfExperience { get; init; }
    public EducationLevel HighestEducation { get; init; }

    /// <summary>Skill name to years of experience, as extracted from the resume.</summary>
    public IReadOnlyDictionary<string, decimal> Skills { get; init; } =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
}
