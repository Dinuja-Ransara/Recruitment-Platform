namespace Meridian.Ai.Models;

/// <summary>
/// The result of scoring one candidate against one posting.
///
/// The score is never returned on its own. Every result carries the factors that
/// produced it, the skills that matched, and the skills that did not, so a
/// recruiter can see why a candidate ranked where they did and defend the
/// decision. A screening tool that cannot explain itself cannot be audited, and
/// in recruitment it cannot be defended against a discrimination challenge.
/// </summary>
public class MatchExplanation
{
    /// <summary>Overall fit, 0 to 100.</summary>
    public double Score { get; init; }

    /// <summary>Which strategy produced this score.</summary>
    public string Strategy { get; init; } = string.Empty;

    /// <summary>Each contributing factor, its raw value, weight and contribution.</summary>
    public IReadOnlyList<ScoreFactor> Factors { get; init; } = Array.Empty<ScoreFactor>();

    /// <summary>Required skills the candidate demonstrably has.</summary>
    public IReadOnlyList<SkillMatch> MatchedSkills { get; init; } = Array.Empty<SkillMatch>();

    /// <summary>Required skills with no evidence in the candidate's resume.</summary>
    public IReadOnlyList<SkillGap> MissingSkills { get; init; } = Array.Empty<SkillGap>();

    /// <summary>
    /// True when the candidate lacks at least one skill the posting marks
    /// mandatory. Surfaced separately because a high similarity score can
    /// otherwise disguise a hard requirement failure.
    /// </summary>
    public bool HasMandatoryGap => MissingSkills.Any(s => s.IsMandatory);

    /// <summary>One-line plain-English summary, shown in list views.</summary>
    public string Summary { get; init; } = string.Empty;
}

/// <summary>
/// A single named input to the score. Value is the raw 0 to 1 measurement,
/// Weight is its share of the total, and Contribution is the product, which is
/// what actually reaches the final number.
/// </summary>
public record ScoreFactor(string Name, double Value, double Weight, string Detail)
{
    public double Contribution => Value * Weight;
}

public record SkillMatch(string Skill, bool IsMandatory, decimal CandidateYears, decimal RequiredYears)
{
    /// <summary>True when the candidate meets the posting's experience bar for this skill.</summary>
    public bool MeetsExperienceBar => CandidateYears >= RequiredYears;
}

public record SkillGap(string Skill, bool IsMandatory, decimal RequiredYears);
