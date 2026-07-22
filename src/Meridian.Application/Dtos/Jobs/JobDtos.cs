using System.ComponentModel.DataAnnotations;
using Meridian.Domain.Enums;

namespace Meridian.Application.Dtos.Jobs;

/// <summary>A posting as shown on the public job board and in search results.</summary>
public record JobSummaryDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
    public string? DepartmentName { get; init; }
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public WorkMode WorkMode { get; init; }
    public EmploymentType EmploymentType { get; init; }
    public SeniorityLevel Seniority { get; init; }
    public decimal MinYearsExperience { get; init; }
    public decimal? SalaryMin { get; init; }
    public decimal? SalaryMax { get; init; }
    public string Currency { get; init; } = "USD";
    public JobStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ClosingDate { get; init; }
    public int ApplicationCount { get; init; }
    public IReadOnlyList<string> RequiredSkills { get; init; } = Array.Empty<string>();
}

/// <summary>Everything about one posting, including its full skill requirements.</summary>
public record JobDetailDto : JobSummaryDto
{
    public string Description { get; init; } = string.Empty;
    public string Responsibilities { get; init; } = string.Empty;
    public EducationLevel RequiredEducation { get; init; }
    public RankingStrategyType RankingStrategy { get; init; }
    public string PostedByName { get; init; } = string.Empty;
    public IReadOnlyList<JobSkillRequirementDto> SkillRequirements { get; init; } =
        Array.Empty<JobSkillRequirementDto>();
}

public record JobSkillRequirementDto
{
    public int SkillId { get; init; }
    public string SkillName { get; init; } = string.Empty;
    public bool IsMandatory { get; init; }
    public int Weight { get; init; }
    public decimal MinYearsExperience { get; init; }
}

public record CreateJobRequest
{
    [Required, MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    public string Responsibilities { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int OrganizationId { get; init; }

    public int? DepartmentId { get; init; }

    [MaxLength(100)]
    public string City { get; init; } = string.Empty;

    [MaxLength(100)]
    public string Country { get; init; } = string.Empty;

    public WorkMode WorkMode { get; init; }
    public EmploymentType EmploymentType { get; init; }
    public SeniorityLevel Seniority { get; init; }

    [Range(0, 50)]
    public decimal MinYearsExperience { get; init; }

    public EducationLevel RequiredEducation { get; init; } = EducationLevel.Unspecified;

    public decimal? SalaryMin { get; init; }
    public decimal? SalaryMax { get; init; }

    [MaxLength(3)]
    public string Currency { get; init; } = "USD";

    public DateTime? ClosingDate { get; init; }

    /// <summary>Which ranking algorithm scores applicants to this posting.</summary>
    public RankingStrategyType RankingStrategy { get; init; } = RankingStrategyType.Hybrid;

    public IReadOnlyList<JobSkillInputDto> Skills { get; init; } = Array.Empty<JobSkillInputDto>();
}

public record UpdateJobRequest : CreateJobRequest
{
    public JobStatus Status { get; init; }
}

public record JobSkillInputDto
{
    [Range(1, int.MaxValue)]
    public int SkillId { get; init; }

    public bool IsMandatory { get; init; }

    [Range(1, 5)]
    public int Weight { get; init; } = 3;

    [Range(0, 50)]
    public decimal MinYearsExperience { get; init; }
}

/// <summary>Filters for the job board. Every field is optional.</summary>
public record JobSearchQuery
{
    public string? Keyword { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public WorkMode? WorkMode { get; init; }
    public EmploymentType? EmploymentType { get; init; }
    public SeniorityLevel? Seniority { get; init; }
    public int? OrganizationId { get; init; }
    public decimal? MaxYearsExperience { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
