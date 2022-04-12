using Haxbot;
using Haxbot.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Data;

public class GamesService
{
    private HaxbotContext Context { get; }

    public GamesService(HaxbotContext context)
    {
        Context = context;
    }

    public IEnumerable<GameModel> GetGames(GamesQueryModel gamesQueryModel)
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
        Func<IQueryable<Game>, IQueryable<Game>> teamFilter = games => gamesQueryModel.Team ? games.WithTeam(GetTeam(players)) : games.WithAny(players);
        return teamFilter(games).Select(game => new GameModel(game));
    }
}
