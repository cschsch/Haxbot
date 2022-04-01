using CLI.Stats;
using Haxbot.Entities;

namespace CLI;

public class StatsPrinter
{
    public bool ByPlayer { get; init; }
    public bool ByTeam { get; init; }
    public bool ByStadium { get; init; }
    public bool ByDay { get; init; }

    public void PrintStats(ICollection<Game> games, ICollection<Player> players)
    {
        var statsCollectors = new List<IStatsCollector>();
        if (ByPlayer) statsCollectors.Add(new PlayerStatsCollector());
        if (ByTeam) statsCollectors.Add(new TeamStatsCollector());
        if (ByStadium) statsCollectors.Add(new StadiumStatsCollector<PlayerStatsCollector>());
        if (ByDay) statsCollectors.Add(new DayStatsCollector<PlayerStatsCollector>());

        Parallel.ForEach(games, game => statsCollectors.ForEach(statsCollector => statsCollector.Register(game, players)));

        var tables = statsCollectors.Select(statsCollector => statsCollector.FormatTable());
        var result = string.Join(Environment.NewLine + Environment.NewLine, tables);
        Console.WriteLine(result);
    }
}
