using Haxbot.Entities;
using Haxbot.Stats;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Haxbot.Stats;

[Parallelizable(ParallelScope.All)]
public class GroupedStatsCollectorTests
{
    [Test]
    public void StadiumStatsCollector_WithStadium_CallsRegister()
    {
        // arrange
        var stadium = new Stadium("haha");
        var game = new Game { Stadium = stadium.Name };
        var statsCollector = new Mock<PlayerStatsCollector>(MockBehavior.Loose);
        var statsCollectors = new ConcurrentDictionary<Stadium, PlayerStatsCollector>();
        var stadiumStatsCollector = new StadiumStatsCollector<PlayerStatsCollector>(statsCollectors, () => statsCollector.Object);

        // act
        stadiumStatsCollector.Register(game, Enumerable.Empty<Player>());

        // assert
        Assert.IsTrue(statsCollectors.ContainsKey(stadium));
        statsCollector.Verify(collector => collector.Register(game, It.IsAny<IEnumerable<Player>>()));
    }

    [Test]
    public void DayStatsCollector_WithStadiumStatsCollector_CallsRegister()
    {
        // arrange
        var game = new Game { Created = DateTime.Today };
        var statsCollector = new Mock<StadiumStatsCollector<TeamStatsCollector>>(MockBehavior.Loose);
        var statsCollectors = new ConcurrentDictionary<DateTime, StadiumStatsCollector<TeamStatsCollector>>();
        var dayStatsCollector = new DayStatsCollector<StadiumStatsCollector<TeamStatsCollector>>(statsCollectors, () => statsCollector.Object);

        // act
        dayStatsCollector.Register(game, Enumerable.Empty<Player>());

        // assert
        Assert.IsTrue(statsCollectors.ContainsKey(DateTime.Today));
        statsCollector.Verify(collector => collector.Register(game, It.IsAny<IEnumerable<Player>>()));
    }

    [Test]
    public void StadiumStatsCollector_Flatten_EnrichesStadium()
    {
        // arrange
        var stats = new GameStats { AmountWon = 5, AmountLost = 1, Identification = "haha" };
        var stadium = new Stadium("hehe");
        var playerStatsCollector = new PlayerStatsCollector(new ConcurrentDictionary<Player, GameStats>(new[] { new KeyValuePair<Player, GameStats>(new Player() { Name = "haha" }, stats) }));
        var statsCollectors = new ConcurrentDictionary<Stadium, PlayerStatsCollector>(new[] { new KeyValuePair<Stadium, PlayerStatsCollector>(stadium, playerStatsCollector) });
        var stadiumStatsCollector = new StadiumStatsCollector<PlayerStatsCollector>(statsCollectors, () => new());

        // act
        var result = stadiumStatsCollector.Flatten();

        // assert
        Assert.AreEqual(new FlattenedGameStats(stats) with { Stadium = stadium }, result.Single());
    }
}
