using Haxbot.Entities;
using System.Collections.Concurrent;

namespace Haxbot.Stats;

public record Stadium(string Name);

public class StadiumStatsCollector<TStatsCollector> : GroupedStatsCollector<Stadium, TStatsCollector>
    where TStatsCollector : IStatsCollector, new()
{
    public StadiumStatsCollector(ConcurrentDictionary<Stadium, TStatsCollector> statsCollectors, Func<TStatsCollector> statsCollectorFactory) : base(statsCollectors, statsCollectorFactory)
    {
    }

    public StadiumStatsCollector() : this(new(), () => new())
    {
    }

    public override Stadium SelectKey(Game game) => new(game.Stadium);

    public override string KeyToString(Stadium key) => key.Name;

    public override IOrderedEnumerable<KeyValuePair<Stadium, TStatsCollector>> OrderStatsCollectors(IEnumerable<KeyValuePair<Stadium, TStatsCollector>> entries) =>
        entries.OrderBy(kv => kv.Key.Name);
}
