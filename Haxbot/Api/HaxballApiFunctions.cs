using Haxbot.Entities;

namespace Haxbot.Api;

public class HaxballApiFunctions
{
    public HaxbotContext Context { get; }
    private Game CurrentGame { get; set; } = new Game();

    public delegate void RoomClosedHandler(object sender, EventArgs e);
    public event RoomClosedHandler? RoomClosed;

    public HaxballApiFunctions(HaxbotContext context)
    {
        Context = context;
    }

    public bool StartGame(HaxballPlayer[] players)
    {
        var red = new Team();
        var blue = new Team();
        CurrentGame = new Game(red, blue, GameState.Undecided);
        
        foreach (var player in players.Where(player => player.Team != TeamId.Spectators))
        {
            var mappedPlayer = Context.Players?.SingleOrDefault(p => player.Auth == p.Auth) ?? new Player(player.Name, player.Auth);
            switch (player.Team)
            {
                case TeamId.Red: red.Players.Add(mappedPlayer); break;
                case TeamId.Blue: blue.Players.Add(mappedPlayer); break;
            }
        }

        Context.Add(CurrentGame);
        Context.SaveChanges();

        return true;
    }

    public bool FinishGame(HaxballScores scores)
    {
        switch (scores)
        {
            case var score when score.Red > score.Blue: CurrentGame.State = GameState.RedWon; break;
            case var score when score.Blue > score.Red: CurrentGame.State = GameState.BlueWon; break;
            default: break;
        }

        Context.SaveChanges();

        return true;
    }

    public void CloseRoom()
    {
        RoomClosed?.Invoke(this, EventArgs.Empty);
    }

    public string HandleCommand(HaxballPlayer player, string message)
    {
        return message;
    }
}