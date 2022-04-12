using Haxbot.Entities;

namespace Haxbot.Stats;

public interface IStatsCollector
{
    void Register(Game game, IEnumerable<Player> players);
    string FormatTable(Func<IEnumerable<GameStats>, string> formatTable);
}