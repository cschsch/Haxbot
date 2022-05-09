using Haxbot.Extensions;
using System.Collections.Immutable;

namespace Haxbot.Api;

public interface IPartyManager
{
    HaxballPlayer[] None(HaxballPlayer[] players);
    HaxballPlayer[] Shuffle(HaxballPlayer[] players);
    HaxballPlayer[] RoundRobin(HaxballPlayer[] players);
}

public class PartyManager : IPartyManager
{
    private Random Random { get; }
    private ImmutableHashSet<HaxballPlayer> Players { get; set; } = ImmutableHashSet<HaxballPlayer>.Empty;
    private Queue<HaxballPlayer[]> TeamSetups { get; } = new Queue<HaxballPlayer[]>();

    public PartyManager(Random random)
    {
        Random = random;
    }

    public PartyManager() : this(new())
    {
    }

    public HaxballPlayer[] None(HaxballPlayer[] players)
    {
        return Array.Empty<HaxballPlayer>();
    }

    public HaxballPlayer[] Shuffle(HaxballPlayer[] players)
    {
        if (players.Length < 2) return Array.Empty<HaxballPlayer>();
        var teamSize = players.Length / 2;
        var shuffled = players.OrderBy(_ => Random.Next()).ToArray();
        var red = SetTeam(shuffled.Take(teamSize), TeamId.Red);
        var blue = SetTeam(shuffled.Skip(teamSize).Take(teamSize), TeamId.Blue);
        return red.Concat(blue).ToArray();
    }

    public HaxballPlayer[] RoundRobin(HaxballPlayer[] players)
    {
        if (players.Length < 2) return Array.Empty<HaxballPlayer>();
        if (!Players.SetEquals(players)) InitTeamSetups(players);

        var next = TeamSetups.Dequeue();
        TeamSetups.Enqueue(next);
        return next;
    }

    private void InitTeamSetups(HaxballPlayer[] players)
    {
        Players = players.ToImmutableHashSet();
        TeamSetups.Clear();
        var teamSize = players.Length / 2;
        var teams = Players.Subsets().Where(team => team.Count == teamSize).ToArray();

        foreach (var red in teams.Take(teams.Length / 2))
        {
            foreach (var blue in teams.Where(team => !team.Intersect(red).Any()))
            {
                TeamSetups.Enqueue(SetTeam(red, TeamId.Red).Concat(SetTeam(blue, TeamId.Blue)).ToArray());
            }
        }

        foreach (var blue in teams.Take(teams.Length / 2))
        {
            foreach (var red in teams.Where(team => !team.Intersect(blue).Any()))
            {
                TeamSetups.Enqueue(SetTeam(red, TeamId.Red).Concat(SetTeam(blue, TeamId.Blue)).ToArray());
            }
        }
    }

    private static IEnumerable<HaxballPlayer> SetTeam(IEnumerable<HaxballPlayer> players, TeamId team) => players.Select(player => player with { Team = team });
}
