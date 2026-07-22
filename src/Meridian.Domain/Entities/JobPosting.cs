using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

/// <summary>
/// A vacancy. Implements the Prototype pattern through <see cref="Clone"/>: a
/// multinational reposts the same role across offices, so recruiters duplicate a
/// posting and edit only the delta rather than retyping it.
/// </summary>
public class JobPosting : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Responsibilities { get; set; } = string.Empty;

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public WorkMode WorkMode { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public SeniorityLevel Seniority { get; set; }

    public decimal MinYearsExperience { get; set; }
    public EducationLevel RequiredEducation { get; set; } = EducationLevel.Unspecified;

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Currency { get; set; } = "USD";

    public JobStatus Status { get; set; } = JobStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosingDate { get; set; }

    /// <summary>Which ranking algorithm scores applicants for this posting.</summary>
    public RankingStrategyType RankingStrategy { get; set; } = RankingStrategyType.Hybrid;

    public int PostedByUserId { get; set; }
    public User PostedBy { get; set; } = null!;

    public ICollection<JobRequiredSkill> RequiredSkills { get; set; } = new List<JobRequiredSkill>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

    /// <summary>
    /// Prototype pattern. Produces a deep copy in Draft status, carrying the skill
    /// requirements across but never the applications or the identity of the original.
    /// </summary>
    public JobPosting Clone()
    {
        var copy = new JobPosting
        {
            Title = Title,
            Description = Description,
            Responsibilities = Responsibilities,
            OrganizationId = OrganizationId,
            DepartmentId = DepartmentId,
            City = City,
            Country = Country,
            WorkMode = WorkMode,
            EmploymentType = EmploymentType,
            Seniority = Seniority,
            MinYearsExperience = MinYearsExperience,
            RequiredEducation = RequiredEducation,
            SalaryMin = SalaryMin,
            SalaryMax = SalaryMax,
            Currency = Currency,
            RankingStrategy = RankingStrategy,
            PostedByUserId = PostedByUserId,
            Status = JobStatus.Draft,
            PublishedAt = null,
            ClosingDate = null
        };

        foreach (var skill in RequiredSkills)
        {
            copy.RequiredSkills.Add(new JobRequiredSkill
            {
                SkillId = skill.SkillId,
                IsMandatory = skill.IsMandatory,
                Weight = skill.Weight,
                MinYearsExperience = skill.MinYearsExperience
            });
        }

        return copy;
    }
}
