using Haxbot.Entities;

namespace CLI.Stats;

public interface IStatsCollector
{
    void Register(Game game, IEnumerable<Player> players);
    string FormatTable();
}