using Haxbot.Entities;
using System.Collections.Concurrent;

namespace Haxbot.Stats;

public class DayStatsCollector<TStatsCollector> : GroupedStatsCollector<DateTime, TStatsCollector>
    where TStatsCollector : IStatsCollector, new()
{
    public DayStatsCollector(ConcurrentDictionary<DateTime, TStatsCollector> statsCollectors, Func<TStatsCollector> statsCollectorFactory) : base(statsCollectors, statsCollectorFactory)
    {
    }

    public DayStatsCollector() : this(new(), () => new())
    {
    }

    public override DateTime SelectKey(Game game) => game.Created.Date;

    public override string KeyToString(DateTime key) => key.ToString();

    public override IOrderedEnumerable<KeyValuePair<DateTime, TStatsCollector>> OrderStatsCollectors(IEnumerable<KeyValuePair<DateTime, TStatsCollector>> entries) =>
        entries.OrderBy(kv => kv.Key);
}