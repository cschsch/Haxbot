using Haxbot.Api;
using Haxbot.Entities;

namespace CLI.Extensions;

public static class GameExtensions
{
    public static GameResult GetResult(this Game game, TeamId teamId)
    {
        return (game.State, teamId) switch
        {
            (GameState.RedWon, TeamId.Red) => GameResult.Won,
            (GameState.BlueWon, TeamId.Red) => GameResult.Lost,
            (GameState.RedWon, TeamId.Blue) => GameResult.Lost,
            (GameState.BlueWon, TeamId.Blue) => GameResult.Won,
            _ => GameResult.Default
        };
    }
}