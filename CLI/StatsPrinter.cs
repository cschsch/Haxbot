using AsciiTableFormatter;
using CLI.Commands;
using Haxbot.Entities;
using Haxbot.Stats;

namespace CLI;

public class StatsPrinter
{
    public Grouping[] Groupings { get; init; } = Array.Empty<Grouping>();

    public void PrintStats(ICollection<Game> games, ICollection<Player> players)
    {
        var collector = GroupingHelper.GetCollector(Groupings);
        Parallel.ForEach(games, game => collector.Register(game, players));
        var result = collector.FormatTable(Formatter.Format);
        Console.WriteLine(result);
    }
}
