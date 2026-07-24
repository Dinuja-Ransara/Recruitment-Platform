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

    /// <summary>
    /// Used by the container and by tests supplying doubles.
    ///
    /// The emptiness check is not defensive padding. A dependency injection
    /// container will happily satisfy IEnumerable&lt;T&gt; with nothing at all when
    /// no implementation is registered, and the failure then surfaces much later
    /// as a missing dictionary key inside scoring. Failing here names the cause.
    /// </summary>
    public RankingStrategyFactory(IEnumerable<IRankingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Type);

        if (_strategies.Count == 0)
        {
            throw new InvalidOperationException(
                "No ranking strategies were supplied. Register the IRankingStrategy implementations "
                + "with the container, or use the parameterless constructor for the built-in set.");
        }

        if (!_strategies.ContainsKey(RankingStrategyType.Hybrid))
        {
            throw new InvalidOperationException(
                "The Hybrid strategy must always be registered: it is the fallback for unrecognised values.");
        }
    }

    public IRankingStrategy Create(RankingStrategyType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
        {
            return strategy;
        }

        // An unrecognised value in the database must not take ranking offline, so
        // the balanced default stands in. The constructor has already guaranteed
        // it is present.
        return _strategies[RankingStrategyType.Hybrid];
    }

    public IReadOnlyList<IRankingStrategy> All() => _strategies.Values.ToList();
}
