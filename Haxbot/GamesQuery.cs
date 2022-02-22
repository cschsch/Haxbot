using Haxbot.Entities;

namespace Haxbot;

public static class GamesQuery
{
    public static IQueryable<Player> ByAuth(this IQueryable<Player> players, IEnumerable<string> auths)
        => players.Where(player => auths.Contains(player.Auth));

    public static IQueryable<Player> ByName(this IQueryable<Player> players, IEnumerable<string> names)
        => players.Where(player => names.Contains(player.Name));

    public static IQueryable<TEntity> Between<TEntity>(this IQueryable<TEntity> entities, DateTime from, DateTime to) where TEntity : Entity
        => entities.Where(entity => from <= entity.Created && to >= entity.Created);

    public static IQueryable<Game> WonBy(this IQueryable<Game> games, IQueryable<Player> players, bool red, bool blue)
    {
        if (red == blue) return games.Where(game => (game.State == GameState.RedWon && game.Red.Players.Any(player => players.Contains(player)))
                                                     || (game.State == GameState.BlueWon && game.Blue.Players.Any(player => players.Contains(player))));
        if (red) return games.Where(game => game.State == GameState.RedWon && game.Red.Players.Any(player => players.Contains(player)));
        return games.Where(game => game.State == GameState.BlueWon && game.Blue.Players.Any(player => players.Contains(player)));
    }

    public static IQueryable<Game> LostBy(this IQueryable<Game> games, IQueryable<Player> players, bool red, bool blue)
    {
        if (red == blue) return games.Where(game => (game.State == GameState.BlueWon && game.Red.Players.Any(player => players.Contains(player)))
                                                     || (game.State == GameState.RedWon && game.Blue.Players.Any(player => players.Contains(player))));
        if (red) return games.Where(game => game.State == GameState.BlueWon && game.Red.Players.Any(player => players.Contains(player)));
        return games.Where(game => game.State == GameState.RedWon && game.Blue.Players.Any(player => players.Contains(player)));
    }

    public static IQueryable<Game> WithAny(this IQueryable<Game> games, IQueryable<Player> players)
        => games.Where(game => players.Any(player => game.Red.Players.Contains(player) || game.Blue.Players.Contains(player)));

    public static IQueryable<Game> WithTeam(this IQueryable<Game> games, IQueryable<Player> team)
    {
        if (!team.Any()) return Enumerable.Empty<Game>().AsQueryable();
        return games.Where(game => team.All(player => game.Red.Players.Contains(player)) || team.All(player => game.Blue.Players.Contains(player)));
    }
}