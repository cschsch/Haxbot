using Haxbot.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haxbot.Api;

public interface IHaxballApiFunctions
{
    void OnPlayerJoin(HaxballPlayer player);
    bool StartGame(string stadium, HaxballPlayer[] players);
    bool FinishGame(HaxballScores scores);
    void CloseRoom();
    string HandleCommand(HaxballPlayer player, string message);
    void SaveReplay(string base64);
}

public class HaxballApiFunctions : IHaxballApiFunctions, IDisposable
{
    private HaxbotContext Context { get; }
    private Game CurrentGame { get; set; } = new Game();

    public delegate void PlayerJoinedHandler(object sender, PlayerJoinedEventArgs e);
    public event PlayerJoinedHandler? PlayerJoined;

    public delegate void RoomClosedHandler(object sender, EventArgs e);
    public event RoomClosedHandler? RoomClosed;

    public HaxballApiFunctions(HaxbotContext context)
    {
        Context = context;
    }

    public void OnPlayerJoin(HaxballPlayer player)
    {
        PlayerJoined?.Invoke(this, new PlayerJoinedEventArgs(player));
    }

    public bool StartGame(string stadium, HaxballPlayer[] players)
    {
        CurrentGame = new Game { State = GameState.Undecided, Stadium = stadium };

        var mappedTeamPlayers = players
            .Where(player => player.Team != TeamId.Spectators)
            .GroupBy(player => player.Team)
            .ToDictionary(team => team.Key, team => team
                .Select(player => Context.Players!
                    .SingleOrDefault(p => player.Auth == p.Auth) ?? new Player(player.Name, player.Auth))
                .OrderBy(player => player.Name));

        foreach (var teamPlayers in mappedTeamPlayers)
        {
            var team = Context.Teams!.Include(team => team.Players)
                    .AsEnumerable()
                    .SingleOrDefault(team => team.Players
                        .OrderBy(player => player.Name)
                        .SequenceEqual(teamPlayers.Value))
                    ?? new Team { Players = new List<Player>(teamPlayers.Value) };
            switch (teamPlayers.Key)
            {
                case TeamId.Red: CurrentGame.Red = team; break;
                case TeamId.Blue: CurrentGame.Blue = team; break;
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
        Dispose();
    }

    public string HandleCommand(HaxballPlayer player, string message)
    {
        return message;
    }

    public void SaveReplay(string base64)
    {
        CurrentGame.Replay = base64;
        Context.SaveChanges();
    }

    public void Dispose() => Context.Dispose();
}