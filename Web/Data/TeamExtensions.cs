using Haxbot.Entities;

namespace Web.Data;

public static class TeamExtensions
{
    public static string GetPlayerNames(this Team team) => string.Join(" ", team.Players.Select(player => player.Name).OrderBy(x => x));
}