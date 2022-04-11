using Haxbot.Entities;

namespace Web.Data;

public class GameModel
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public string Stadium { get; set; } = string.Empty;
    public Team? WinningTeam { get; }
    public Team? LosingTeam { get; }
    public Team Red { get; set; }
    public Team Blue { get; set; }
    public GameState State { get; set; }
    public string Replay { get; set; }

    public GameModel(Game game)
    {
        Id = game.Id;
        Created = game.Created;
        Stadium = game.Stadium;
        WinningTeam = game.State switch
        {
            GameState.RedWon => game.Red,
            GameState.BlueWon => game.Blue,
            _ => null
        };
        LosingTeam = game.State switch
        {
            GameState.RedWon => game.Blue,
            GameState.BlueWon => game.Red,
            _ => null
        };
        Red = game.Red;
        Blue = game.Blue;
        State = game.State;
        Replay = game.Replay;
    }
}
