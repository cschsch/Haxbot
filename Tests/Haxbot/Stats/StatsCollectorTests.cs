using Haxbot.Entities;
using Haxbot.Stats;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Haxbot.Stats;

[Parallelizable(ParallelScope.All)]
public class StatsCollectorTests
{
    [Test]
    public void PlayerStatsCollector_WithWinnerAndLoser_IsRegistered()
    {
        // arrange
        var schoko = new Player { Name = "schoko" };
        var blanko = new Player { Name = "blanko" };
        var game = new Game 
        { 
            Red = new Team { Players = new [] { schoko } },
            Blue = new Team { Players = new [] { blanko } },
            State = GameState.RedWon
        };

        var stats = new ConcurrentDictionary<Player, GameStats>();
        var playerStatsCollector = new PlayerStatsCollector(stats);

        // act
        playerStatsCollector.Register(game, new[] { schoko, blanko });

        // assert
        Assert.AreEqual(1, stats[schoko].AmountWon);
        Assert.AreEqual(1, stats[blanko].AmountLost);
    }

    [Test]
    public void PlayerStatsCollector_WithWinnerOnly_IsOnlyWinnerRegistered()
    {
        // arrange
        var schoko = new Player { Name = "schoko" };
        var blanko = new Player { Name = "blanko" };
        var game = new Game
        {
            Red = new Team { Players = new[] { schoko } },
            Blue = new Team { Players = new[] { blanko } },
            State = GameState.RedWon
        };

        var stats = new ConcurrentDictionary<Player, GameStats>();
        var playerStatsCollector = new PlayerStatsCollector(stats);

        // act
        playerStatsCollector.Register(game, new[] { schoko });

        // assert
        Assert.AreEqual(1, stats[schoko].AmountWon);
        Assert.IsFalse(stats.ContainsKey(blanko));
    }

    [Test]
    public void PlayerStatsCollector_WithUndecided_IsNotRegistered()
    {
        // arrange
        var schoko = new Player { Name = "schoko" };
        var blanko = new Player { Name = "blanko" };
        var game = new Game
        {
            Red = new Team { Players = new[] { schoko } },
            Blue = new Team { Players = new[] { blanko } },
            State = GameState.Undecided
        };

        var stats = new ConcurrentDictionary<Player, GameStats>();
        var playerStatsCollector = new PlayerStatsCollector(stats);

        // act
        playerStatsCollector.Register(game, new[] { schoko, blanko });

        // assert
        CollectionAssert.IsEmpty(stats);
    }

    [Test]
    public void TeamStatsCollector_WithTeamsComplete_IsRegistered()
    {
        // arrange
        var red = new Team { Players = new[] { new Player { Name = "schoko" } } };
        var blue = new Team { Players = new[] { new Player { Name = "blanko" } } };
        var game = new Game
        {
            Red = red,
            Blue = blue,
            State = GameState.RedWon
        };

        var stats = new ConcurrentDictionary<Team, GameStats>();
        var teamStatsCollector = new TeamStatsCollector(stats);

        // act
        teamStatsCollector.Register(game, red.Players.Concat(blue.Players));

        // assert
        Assert.AreEqual(1, stats[red].AmountWon);
        Assert.AreEqual(1, stats[blue].AmountLost);
    }

    [Test]
    public void TeamStatsCollector_RedTeamIncomplete_IsNotRegistered()
    {
        // arrange
        var red = new Team { Players = new[] { new Player { Name = "schoko" }, new Player { Name = "kranko" } } };
        var blue = new Team { Players = new[] { new Player { Name = "blanko" } } };
        var game = new Game
        {
            Red = red,
            Blue = blue,
            State = GameState.RedWon
        };

        var stats = new ConcurrentDictionary<Team, GameStats>();
        var teamStatsCollector = new TeamStatsCollector(stats);

        // act
        teamStatsCollector.Register(game, red.Players.Take(1).Concat(blue.Players));

        // assert
        Assert.IsFalse(stats.ContainsKey(red));
        Assert.AreEqual(1, stats[blue].AmountLost);
    }

    [Test]
    public void TeamStatsCollector_WithUndecided_IsNotRegistered()
    {
        // arrange
        var red = new Team { Players = new[] { new Player { Name = "schoko" } } };
        var blue = new Team { Players = new[] { new Player { Name = "blanko" } } };
        var game = new Game
        {
            Red = red,
            Blue = blue,
            State = GameState.Undecided
        };

        var stats = new ConcurrentDictionary<Team, GameStats>();
        var teamStatsCollector = new TeamStatsCollector(stats);

        // act
        teamStatsCollector.Register(game, red.Players.Concat(blue.Players));

        // assert
        CollectionAssert.IsEmpty(stats);
    }

    [Test]
    public void PlayerStatsCollector_Flatten_IsSameButFlattened()
    {
        // arrange
        var stats = new GameStats { AmountWon = 5, AmountLost = 1, Identification = "haha" };
        var playerStatsCollector = new PlayerStatsCollector(new ConcurrentDictionary<Player, GameStats>(new[] { new KeyValuePair<Player, GameStats>(new Player() { Name = "haha" }, stats) }));

        // act
        var result = playerStatsCollector.Flatten();

        // assert
        Assert.AreEqual(new FlattenedGameStats(stats), result.Single());
    }
}
