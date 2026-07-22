using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

/// <summary>
/// A skill the extractor found in a resume, with the evidence it based that on.
/// ConfidenceScore and EvidenceSnippet exist so the match explanation can cite a
/// reason rather than asserting a number.
/// </summary>
public class ResumeSkill : BaseEntity
{
    public int ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public decimal YearsOfExperience { get; set; }
    public double ConfidenceScore { get; set; }
    public string? EvidenceSnippet { get; set; }
}
