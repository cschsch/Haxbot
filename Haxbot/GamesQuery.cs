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

    public static IEnumerable<Game> WonBy(this IEnumerable<Game> games, IQueryable<Player> players, bool red, bool blue)
    {
        bool AnyPlayerWonRed(Game game) => game.State == GameState.RedWon && game.Red.Players.Any(players.Contains);
        bool AnyPlayerWonBlue(Game game) => game.State == GameState.BlueWon && game.Blue.Players.Any(players.Contains);

        if (red == blue) return games.Where(game => AnyPlayerWonRed(game) || AnyPlayerWonBlue(game));
        if (red) return games.Where(AnyPlayerWonRed);
        return games.Where(AnyPlayerWonBlue);
    }

    public static IEnumerable<Game> LostBy(this IEnumerable<Game> games, IQueryable<Player> players, bool red, bool blue)
    {
        bool AnyPlayerLostBlue(Game game) => game.State == GameState.RedWon && game.Blue.Players.Any(players.Contains);
        bool AnyPlayerLostRed(Game game) => game.State == GameState.BlueWon && game.Red.Players.Any(players.Contains);

        if (red == blue) return games.Where(game => AnyPlayerLostBlue(game) || AnyPlayerLostRed(game));
        if (red) return games.Where(AnyPlayerLostRed);
        return games.Where(AnyPlayerLostBlue);
    }

    public static IQueryable<Game> WithAny(this IQueryable<Game> games, IQueryable<Player> players)
        => games.Where(game => players.Any(player => game.Red.Players.Contains(player) || game.Blue.Players.Contains(player)));

    public static IQueryable<Game> WithTeam(this IQueryable<Game> games, IQueryable<Player> team)
    {
        if (!team.Any()) return Enumerable.Empty<Game>().AsQueryable();
        return games.Where(game => team.All(player => game.Red.Players.Contains(player)) || team.All(player => game.Blue.Players.Contains(player)));
    }

    public static IQueryable<Game> PlayedOn(this IQueryable<Game> games, string stadium) => 
        games.Where(game => game.Stadium.Contains(stadium, StringComparison.OrdinalIgnoreCase));
}