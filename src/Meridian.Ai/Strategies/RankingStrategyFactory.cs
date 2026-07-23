using Meridian.Domain.Enums;

namespace Meridian.Ai.Strategies;

/// <summary>
/// Factory Method. Resolves the strategy a posting asked for.
///
/// The alternative would be a switch statement wherever ranking happens, which
/// would need editing in several places every time a strategy is added. Here a
/// new strategy is registered once and every call site picks it up.
/// </summary>
public interface IRankingStrategyFactory
{
    IRankingStrategy Create(RankingStrategyType type);
    IReadOnlyList<IRankingStrategy> All();
}

public class RankingStrategyFactory : IRankingStrategyFactory
{
    private readonly Dictionary<RankingStrategyType, IRankingStrategy> _strategies;

    public RankingStrategyFactory()
        : this(new IRankingStrategy[]
        {
            new SkillWeightedStrategy(),
            new TfIdfSimilarityStrategy(),
            new HybridStrategy(),
            new ExperienceFirstStrategy()
        })
    {
    }

    /// <summary>Injection point used by the tests to supply doubles.</summary>
    public RankingStrategyFactory(IEnumerable<IRankingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Type);
    }

    public IRankingStrategy Create(RankingStrategyType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
        {
            return strategy;
        }

        // An unknown value in the database must not take ranking offline, so the
        // balanced default stands in rather than throwing.
        return _strategies[RankingStrategyType.Hybrid];
    }

    public IReadOnlyList<IRankingStrategy> All() => _strategies.Values.ToList();
}
