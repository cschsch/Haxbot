using CLI.Stats;
using Haxbot.Entities;
using System.Reflection;

namespace CLI;

public class StatsPrinter
{
    public static Grouping[] ResultGroupings { get; } = new Grouping[] { Grouping.Player, Grouping.Team };
    public Grouping[] Groupings { get; init; } = Array.Empty<Grouping>();

    private IStatsCollector GetCollector()
    {
        if (!ResultGroupings.Contains(Groupings.Last())) throw new ArgumentException("Last Grouping needs to be of a result collector");
        if (Groupings.Count(ResultGroupings.Contains) > 1) throw new ArgumentException("There can only be one result collector");

        var collectorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.Namespace == "CLI.Stats" && !type.IsAbstract && !type.IsInterface && type.Name.Contains("StatsCollector"))
            .ToArray();
        var completeCollectorType = Groupings
            .Select(grouping => collectorTypes.Single(type => type.Name.StartsWith(grouping.ToString())))
            .Reverse()
            .Aggregate((acc, cur) => cur.MakeGenericType(acc));
        return (IStatsCollector) Activator.CreateInstance(completeCollectorType)!;
    }

    public void PrintStats(ICollection<Game> games, ICollection<Player> players)
    {
        var collector = GetCollector();
        Parallel.ForEach(games, game => collector.Register(game, players));
        var result = collector.FormatTable();
        Console.WriteLine(result);
    }
}
