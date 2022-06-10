namespace Haxbot.Stats;

public static class GroupingHelper
{
    public static Grouping[] ResultGroupings { get; } = new Grouping[] { Grouping.Player, Grouping.Team };

    public static IStatsCollector GetCollector(Grouping[] groupings)
    {
        if (!ResultGroupings.Contains(groupings.Last())) throw new ArgumentException("Last Grouping needs to be of a result collector");
        if (groupings.Count(ResultGroupings.Contains) > 1) throw new ArgumentException("There can only be one result collector");

        var collectorTypes = typeof(IStatsCollector)
            .Assembly
            .GetTypes()
            .Where(type => type.Namespace == "Haxbot.Stats" && !type.IsAbstract && !type.IsInterface && type.Name.Contains("StatsCollector"))
            .ToArray();
        var completeCollectorType = groupings
            .Distinct()
            .Select(grouping => collectorTypes.Single(type => type.Name.StartsWith(grouping.ToString())))
            .Reverse()
            .Aggregate((acc, cur) => cur.MakeGenericType(acc));
        return (IStatsCollector)Activator.CreateInstance(completeCollectorType)!;
    }
}