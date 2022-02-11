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

    public Game(Team red, Team blue, GameState state) : base()
    {
        Red = red;
        Blue = blue;
        State = state;
    }

    public Game() : this(new Team(), new Team(), GameState.Default) { }
}