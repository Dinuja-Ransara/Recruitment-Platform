using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// A skill a posting asks for. Weight and IsMandatory are the inputs the ranking
/// strategies use to separate "must have" from "nice to have".
/// </summary>
public class JobRequiredSkill : BaseEntity
{
    public int JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public bool IsMandatory { get; set; }

    /// <summary>Relative importance, 1 to 5. Normalised by the strategy before scoring.</summary>
    public int Weight { get; set; } = 3;

    public decimal MinYearsExperience { get; set; }
}
