using AsciiTableFormatter;
using Haxbot.Entities;
using System.Collections.Concurrent;

namespace CLI.Stats;

public interface IStatsCollector<TKey> : IStatsCollector
{
    IEnumerable<ResultMap<TKey>> SelectKeys(Game game, IEnumerable<Player> players);
    GameStats StatsFactory(ResultMap<TKey> resultMap);
}

public abstract class StatsCollector<TKey> : IStatsCollector<TKey> 
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, GameStats> _stats;

    protected StatsCollector(ConcurrentDictionary<TKey, GameStats> stats)
    {
        _stats = stats;
    }

    public virtual void Register(Game game, IEnumerable<Player> players)
    {
        foreach (var resultMap in SelectKeys(game, players))
        {
            _stats.AddOrUpdate(
                resultMap.Key,
                StatsFactory(resultMap),
                (_, stats) => stats.RegisterGame(resultMap.Result));
        }
    }

    public string FormatTable()
    {
        var entries = _stats.Values
            .OrderByDescending(stats => stats.Winrate)
            .ThenByDescending(stats => stats.AmountPlayed)
            .ThenBy(stats => stats.Identification);
        return entries.Any() ? Formatter.Format(entries) : string.Empty;
    }

    public abstract IEnumerable<ResultMap<TKey>> SelectKeys(Game game, IEnumerable<Player> players);
    public abstract GameStats StatsFactory(ResultMap<TKey> resultMap);
}