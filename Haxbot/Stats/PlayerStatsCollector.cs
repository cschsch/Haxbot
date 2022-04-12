using Haxbot.Api;
using Haxbot.Entities;
using Haxbot.Extensions;
using System.Collections.Concurrent;

namespace Haxbot.Stats;

public class PlayerStatsCollector : StatsCollector<Player>
{
    public PlayerStatsCollector(ConcurrentDictionary<Player, GameStats> stats) : base(stats)
    {
    }

    public PlayerStatsCollector() : this(new())
    {
    }

    public override IEnumerable<ResultMap<Player>> SelectKeys(Game game, IEnumerable<Player> players)
    {
        var red = game.Red.Players
            .Intersect(players)
            .Select(player => new ResultMap<Player>(player, game.GetResult(TeamId.Red)));
        var blue = game.Blue.Players
            .Intersect(players)
            .Select(player => new ResultMap<Player>(player, game.GetResult(TeamId.Blue)));
        return red.Concat(blue).Where(resultMap => resultMap.Result != GameResult.Default);
    }

    public override GameStats StatsFactory(ResultMap<Player> resultMap)
    {
        var gameStats = new GameStats { Identification = resultMap.Key.Name };
        return gameStats.RegisterGame(resultMap.Result);
    }
}