namespace Haxbot.Api;

public interface IPartyManager
{
    TeamSetup None(HaxballPlayer[] players);
    TeamSetup Shuffle(HaxballPlayer[] players);
    TeamSetup RoundRobin(HaxballPlayer[] players);
}

public class PartyManager : IPartyManager
{
    private Random Random { get; }
    private TeamSetup LastTeamSetup { get; set; } = TeamSetup.Default;

    public PartyManager(Random random)
    {
        Random = random;
    }

    public PartyManager() : this(new())
    {
    }

    public TeamSetup None(HaxballPlayer[] players)
    {
        return TeamSetup.Default;
    }

    public TeamSetup Shuffle(HaxballPlayer[] players)
    {
        if (players.Length < 2) return TeamSetup.Default;
        var teamSize = players.Length / 2;
        var shuffled = players.OrderBy(_ => Random.Next()).ToArray();
        var red = shuffled.Take(teamSize).ToArray();
        var blue = shuffled.Skip(teamSize).Take(teamSize).ToArray();
        return new(red, blue);
    }

    public TeamSetup RoundRobin(HaxballPlayer[] players)
    {
        if (players.Length < 2) return TeamSetup.Default;
        var newSetup = (LastTeamSetup == TeamSetup.Default || LastTeamSetup.TotalCount != players.Length) ? Shuffle(players) : NextCycle();
        LastTeamSetup = newSetup;
        return newSetup;
    }

    private TeamSetup NextCycle()
    {
        var red = LastTeamSetup.Red.Prepend(LastTeamSetup.Blue.First()).Take(LastTeamSetup.Red.Length).ToArray();
        var blue = LastTeamSetup.Blue.Append(LastTeamSetup.Red.Last()).Skip(1).ToArray();
        return new(red, blue);
    }
}
