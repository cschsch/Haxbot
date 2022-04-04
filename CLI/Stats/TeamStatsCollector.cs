using CLI.Commands;
using CLI.Extensions;
using Haxbot.Api;
using Haxbot.Entities;
using System.Collections.Concurrent;

namespace CLI.Stats;

public class TeamStatsCollector : StatsCollector<Team>
{
    public TeamStatsCollector(ConcurrentDictionary<Team, GameStats> stats) : base(stats)
    {
    }

    public TeamStatsCollector() : this(new())
    {
    }

    public override IEnumerable<ResultMap<Team>> SelectKeys(Game game, IEnumerable<Player> players)
    {
        var red = new ResultMap<Team>(game.Red, game.GetResult(TeamId.Red));
        var blue = new ResultMap<Team>(game.Blue, game.GetResult(TeamId.Blue));
        return new[] { red, blue }.Where(resultMap => resultMap.Key.Players.All(players.Contains) && resultMap.Result != GameResult.Default);
    }

    public override GameStats StatsFactory(ResultMap<Team> resultMap)
    {
        var identification = string.Join(" ", resultMap.Key.Players.Select(player => player.Name));
        var gameStats = new GameStats { Identification = identification };
        return gameStats.RegisterGame(resultMap.Result);
    }
}