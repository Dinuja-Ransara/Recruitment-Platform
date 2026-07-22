namespace Meridian.Domain.Enums;

/// <summary>
/// Selects which IRankingStrategy implementation scores applicants for a posting.
/// Chosen per job posting by the recruiter, resolved at runtime by the Strategy pattern.
/// </summary>
public enum RankingStrategyType
{
    SkillWeighted = 0,
    TfIdfSimilarity = 1,
    Hybrid = 2,
    ExperienceFirst = 3
}
