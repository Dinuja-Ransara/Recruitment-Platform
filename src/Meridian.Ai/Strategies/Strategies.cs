using Meridian.Domain.Enums;

namespace Meridian.Ai.Strategies;

/// <summary>
/// Decides almost entirely on evidenced skills. The default for technical roles
/// where the requirement list is precise and non-negotiable.
/// </summary>
public class SkillWeightedStrategy : WeightedStrategyBase
{
    public override RankingStrategyType Type => RankingStrategyType.SkillWeighted;
    public override string DisplayName => "Skill weighted";

    public override string WhenToUse =>
        "Technical roles with a precise requirement list. Rewards evidenced skills "
        + "and treats the written CV as secondary.";

    protected override FactorWeights Weights { get; } =
        new(Skills: 0.65, Similarity: 0.10, Experience: 0.15, Education: 0.05, Location: 0.05);
}

/// <summary>
/// Decides mostly on the language of the resume against the language of the
/// posting. Useful where the taxonomy cannot capture the requirement, such as
/// domain or industry experience.
/// </summary>
public class TfIdfSimilarityStrategy : WeightedStrategyBase
{
    public override RankingStrategyType Type => RankingStrategyType.TfIdfSimilarity;
    public override string DisplayName => "Resume similarity";

    public override string WhenToUse =>
        "Roles defined by domain rather than tooling, where the important experience "
        + "is described in prose and no skill list would capture it.";

    protected override FactorWeights Weights { get; } =
        new(Skills: 0.20, Similarity: 0.55, Experience: 0.15, Education: 0.05, Location: 0.05);
}

/// <summary>
/// Balanced default. Skills lead, but the resume text, experience and
/// practicalities all carry real weight.
/// </summary>
public class HybridStrategy : WeightedStrategyBase
{
    public override RankingStrategyType Type => RankingStrategyType.Hybrid;
    public override string DisplayName => "Hybrid";

    public override string WhenToUse =>
        "The general purpose default. Balances evidenced skills against resume "
        + "relevance, experience and location.";

    protected override FactorWeights Weights { get; } =
        new(Skills: 0.40, Similarity: 0.25, Experience: 0.20, Education: 0.07, Location: 0.08);
}

/// <summary>
/// Leads on depth of experience. Intended for senior and leadership hires, where
/// the number of years and the seniority of the narrative matter more than
/// whether a particular framework appears on the CV.
/// </summary>
public class ExperienceFirstStrategy : WeightedStrategyBase
{
    public override RankingStrategyType Type => RankingStrategyType.ExperienceFirst;
    public override string DisplayName => "Experience first";

    public override string WhenToUse =>
        "Senior and leadership hires, where depth of experience decides the outcome "
        + "and specific tools are learnable on the job.";

    protected override FactorWeights Weights { get; } =
        new(Skills: 0.25, Similarity: 0.20, Experience: 0.45, Education: 0.05, Location: 0.05);
}
