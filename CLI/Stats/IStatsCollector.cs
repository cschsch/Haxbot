using Haxbot.Entities;

namespace CLI.Stats;

public record ResultMap<TKey>(TKey Key, GameResult Result);

public interface IStatsCollector
{
    void Register(Game game, IEnumerable<Player> players);
    string FormatTable();
}