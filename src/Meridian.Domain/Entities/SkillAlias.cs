using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

public class SkillAlias : BaseEntity
{
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public string Alias { get; set; } = string.Empty;
}
