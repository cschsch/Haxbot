using System.ComponentModel.DataAnnotations;

namespace Haxbot;

public enum GameState
{
    Default,
    Undecided,
    RedWon,
    BlueWon
}

public class Game : Entity
{
    [Required]
    public Team Red { get; set; }
    [Required]
    public Team Blue { get; set; }
    [Required]
    public GameState State { get; set; }

    public Game(Team red, Team blue) : base()
    {
        Red = red;
        Blue = blue;
    }

    public Game() : this(new Team(), new Team()) { }
}