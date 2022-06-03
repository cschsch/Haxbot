using Haxbot;
using Haxbot.Entities;
using Haxbot.Stats;
using Microsoft.EntityFrameworkCore;

namespace Web.Data;

public class GamesService
{
    private HaxbotContext Context { get; }

    public GamesService(HaxbotContext context)
    {
        Context = context;
    }

    public IEnumerable<Game> GetGames(GamesQueryModel gamesQueryModel)
    {
        Team GetTeam(IQueryable<Player> players)
        {
            var teamPlayers = players.ToArray();
            var team = Context.Teams!.Include(team => team.Players).AsEnumerable().SingleOrDefault(team => team.Players.Count == teamPlayers.Length && teamPlayers.All(player => team.Players.Contains(player)));
            return team ?? new Team();
        }

        Func<IEnumerable<string>, IQueryable<Player>> playerFilter = gamesQueryModel.Auth ? Context.Players!.ByAuth : Context.Players!.ByName;
        var players = gamesQueryModel.Players.Any() ? playerFilter(gamesQueryModel.Players) : Context.Players!;
        var games = Context.Games!
            .Between(gamesQueryModel.From ?? DateTime.MinValue, gamesQueryModel.To ?? DateTime.MaxValue)
            .PlayedOn(gamesQueryModel.Stadium)
            .Where(game => game.State != GameState.Undecided || gamesQueryModel.Undecided)
            .Include(game => game.Red.Players).Include(game => game.Blue.Players);
        return gamesQueryModel.Team ? games.WithTeam(GetTeam(players)) : games.WithAny(players);
    }

    public IEnumerable<FlattenedGameStats> CollectStats<TCollector>(IEnumerable<Game> games) 
        where TCollector : IStatsCollector, new()
    {
        var collector = new TCollector();
        foreach (var game in games)
        {
            collector.Register(game, Context.Players!);
        }
        return collector.Flatten();
    }

    public IEnumerable<Player> GetPlayers()
    {
        return Context.Players!.AsNoTracking();
    }
}
