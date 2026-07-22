using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// A canonical skill. Aliases resolve the many ways the same skill is written on
/// a CV ("JS", "Javascript", "ECMAScript") back to one row.
/// </summary>
public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public ICollection<SkillAlias> Aliases { get; set; } = new List<SkillAlias>();
    public ICollection<ResumeSkill> ResumeSkills { get; set; } = new List<ResumeSkill>();
    public ICollection<JobRequiredSkill> JobRequiredSkills { get; set; } = new List<JobRequiredSkill>();
}
