using System.ComponentModel.DataAnnotations;

namespace Haxbot.Entities;

public class Game : Entity
{
    [Required]
    public Team Red { get; set; }
    [Required]
    public Team Blue { get; set; }
    [Required]
    public GameState State { get; set; }
    public string Stadium { get; set; }
    public string Replay { get; set; }

    public Game(Team red, Team blue, GameState state, string stadium, string replay) : base()
    {
        Red = red;
        Blue = blue;
        State = state;
        Stadium = stadium;
        Replay = replay;
    }

    public Game() : this(new Team(), new Team(), GameState.Default, string.Empty, string.Empty) { }
}