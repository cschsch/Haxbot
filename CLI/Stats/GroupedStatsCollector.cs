using CLI.Extensions;
using Haxbot.Entities;
using System.Collections.Concurrent;

namespace CLI.Stats;

public interface IGroupedStatsCollector<TKey, TStatsCollector> : IStatsCollector
{
    Func<TStatsCollector> StatsCollectorFactory { get; }
    TKey SelectKey(Game game);
}

public abstract class GroupedStatsCollector<TKey, TStatsCollector> : IGroupedStatsCollector<TKey, TStatsCollector>
    where TKey : notnull
    where TStatsCollector : IStatsCollector
{
    private readonly ConcurrentDictionary<TKey, TStatsCollector> _statsCollectors;
    public Func<TStatsCollector> StatsCollectorFactory { get; }

    protected GroupedStatsCollector(ConcurrentDictionary<TKey, TStatsCollector> statsCollectors, Func<TStatsCollector> statsCollectorFactory)
    {
        _statsCollectors = statsCollectors;
        StatsCollectorFactory = statsCollectorFactory;
    }

    public virtual void Register(Game game, IEnumerable<Player> players)
    {
        var collector = _statsCollectors.GetOrAdd(SelectKey(game), StatsCollectorFactory());
        collector.Register(game, players);
    }

    public string FormatTable()
    {
        Func<string, string, string> join;

        if (typeof(TStatsCollector).IsAssignableToGenericType(typeof(IGroupedStatsCollector<,>)))
        {
            join = (key, value) =>
            {
                var longestLineLength = value.Split('\n').Max(str => str.Length);
                var amountOfPlusSigns = (longestLineLength - key.Length - 2) / 2;
                var bannerHalf = string.Concat(Enumerable.Repeat('+', amountOfPlusSigns));
                return
$@"{bannerHalf} {key} {bannerHalf}

{value}";
            };
        } else if (typeof(TStatsCollector).IsAssignableToGenericType(typeof(IStatsCollector<>)))
        {
            join = (key, value) => string.Join(Environment.NewLine, key, value);
        } else
        {
            join = string.Concat;
        }

        var entries = OrderStatsCollectors(_statsCollectors).Select(statsCollector =>
        {
            var key = KeyToString(statsCollector.Key);
            var value = statsCollector.Value.FormatTable();
            return join(key, value);
        });
        return string.Join(Environment.NewLine + Environment.NewLine, entries);
    }

    public abstract TKey SelectKey(Game game);
    public abstract string KeyToString(TKey key);
    public abstract IOrderedEnumerable<KeyValuePair<TKey, TStatsCollector>> OrderStatsCollectors(IEnumerable<KeyValuePair<TKey, TStatsCollector>> entries);
}