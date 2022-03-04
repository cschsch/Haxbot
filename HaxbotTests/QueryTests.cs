using Haxbot;
using Haxbot.Entities;
using Haxbot.Settings;
using NUnit.Framework;
using System;
using System.Linq;

namespace HaxbotTests;

[Parallelizable(ParallelScope.All)]
public class QueryTests
{
    [Test]
    public void ByAuth_NoAuthsGiven_IsEmpty()
    {
        // arrange
        var players = new[] { new Player { Auth = "auth" } };

        // act
        var result = players.AsQueryable().ByAuth(Enumerable.Empty<string>());

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void ByAuth_AuthsGiven_ReturnsMatchingPlayers()
    {
        // arrange
        var players = Enumerable.Range(0, 10).Select(x => new Player { Auth = x.ToString() }).ToArray();
        var expectedIndexes = new[] { 0, 3, 8 };
        var expected = expectedIndexes.Select(index => players[index]);
        var auths = expectedIndexes.Select(index => index.ToString());

        // act
        var result = players.AsQueryable().ByAuth(auths);

        // assert
        CollectionAssert.AreEqual(expected, result);
    }

    [Test]
    public void ByName_NoNamesGiven_IsEmpty()
    {
        // arrange
        var players = new[] { new Player { Name = "name" } };

        // act
        var result = players.AsQueryable().ByName(Enumerable.Empty<string>());

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void ByName_NamesGiven_ReturnsMatchingPlayers()
    {
        // arrange
        var players = Enumerable.Range(0, 10).Select(x => new Player { Name = x.ToString() }).ToArray();
        var expectedIndexes = new[] { 0, 3, 8 };
        var expected = expectedIndexes.Select(index => players[index]);
        var names = expectedIndexes.Select(index => index.ToString());

        // act
        var result = players.AsQueryable().ByName(names);

        // assert
        CollectionAssert.AreEqual(expected, result);
    }

    [Test]
    public void Between_FromLargerThanTo_ReturnsNothing()
    {
        // arrange
        var games = new [] { new Game { Created = DateTime.MinValue }, new Game { Created = DateTime.Today }, new Game { Created = DateTime.MaxValue } };

        // act
        var result = games.AsQueryable().Between(DateTime.MaxValue, DateTime.MinValue);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void Between_MultipleEntities_ReturnsEntitiesInRange()
    {
        // arrange
        var players = new[] { DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-2), DateTime.Today.AddDays(1), DateTime.Today.AddDays(10) }.Select(date => new Player { Created = date }).ToArray();
        var expected = players.Skip(1).Take(2);

        // act
        var result = players.AsQueryable().Between(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(2));

        // assert
        CollectionAssert.AreEqual(expected, result);
    }

    [Test]
    public void Between_SameDates_ReturnsEntities()
    {
        // arrange
        var expected = new[] { new Team { Created = DateTime.Today.AddDays(-1) }, new Team { Created = DateTime.Today } };
        
        // act
        var result = expected.AsQueryable().Between(DateTime.Today.AddDays(-1), DateTime.Today);

        // assert
        CollectionAssert.AreEqual(expected, result);
    }

    [Test]
    public void Between_OneMillisecondApart_IsEmpty()
    {
        // arrange
        var players = new[] { new Player { Created = DateTime.Today }, new Player { Created = DateTime.Today.AddDays(1) } };

        // act
        var result = players.AsQueryable().Between(DateTime.Today.AddMilliseconds(1), DateTime.Today.AddDays(1).AddMilliseconds(-1));

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WonBy_PlayersEmpty_IsEmpty()
    {
        // arrange
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.WonBy(Enumerable.Empty<Player>().AsQueryable(), false, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WonBy_PlayerNotInGame_IsEmpty()
    {
        // arrange
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.WonBy(new [] { new Player() }.AsQueryable(), false, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WonBy_PlayerInLosingTeam_IsEmpty()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(player);

        // act
        var result = new[] { game }.WonBy(new[] { player }.AsQueryable(), false, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WonBy_PlayerInWinningTeam_IsGame()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(player);
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.WonBy(new[] { player }.AsQueryable(), false, false);

        // assert
        CollectionAssert.AreEqual(new [] { game }, result);
    }

    [Test]
    public void WonBy_PlayerInWinningTeam_OnlyBlueWins_IsEmpty()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(player);
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.WonBy(new[] { player }.AsQueryable(), false, true);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WonBy_PlayerInWinningTeam_OnlyRedWins_IsGame()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(player);
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.WonBy(new[] { player }.AsQueryable(), true, false);

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void WonBy_PlayersInBothTeams_IsGame()
    {
        // arrange
        var winner = new Player();
        var loser = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(winner);
        game.Blue.Players.Add(loser);

        // act
        var result = new[] { game }.WonBy(new[] { winner, loser }.AsQueryable(), false, false);

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void WonBy_PlayerInLosingTeam_OnlyRedWins_IsEmpty()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(player);

        // act
        var result = new[] { game }.WonBy(new[] { player }.AsQueryable(), true, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WonBy_PlayersInBothTeams_OnlyRedAndOnlyBlueWins_IsGame()
    {
        // arrange
        var winner = new Player();
        var loser = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(winner);
        game.Blue.Players.Add(loser);

        // act
        var result = new[] { game }.WonBy(new[] { winner, loser }.AsQueryable(), true, true);

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void LostBy_PlayersEmpty_IsEmpty()
    {
        // arrange
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.LostBy(Enumerable.Empty<Player>().AsQueryable(), false, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void LostBy_PlayerNotInGame_IsEmpty()
    {
        // arrange
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.LostBy(new[] { new Player() }.AsQueryable(), false, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void LostBy_PlayerInLosingTeam_IsGame()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(player);

        // act
        var result = new[] { game }.LostBy(new[] { player }.AsQueryable(), false, false);

        // assert
        CollectionAssert.AreEqual(new [] { game }, result);
    }

    [Test]
    public void LostBy_PlayerInWinningTeam_IsEmpty()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(player);
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.LostBy(new[] { player }.AsQueryable(), false, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void LostBy_PlayerInWinningTeam_OnlyBlueLosses_IsEmpty()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(player);
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.LostBy(new[] { player }.AsQueryable(), false, true);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void LostBy_PlayerInLosingTeam_OnlyRedLosses_IsGame()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.BlueWon };
        game.Red.Players.Add(player);
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.LostBy(new[] { player }.AsQueryable(), true, false);

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void LostBy_PlayersInBothTeams_IsGame()
    {
        // arrange
        var winner = new Player();
        var loser = new Player();
        var game = new Game { State = GameState.BlueWon };
        game.Red.Players.Add(winner);
        game.Blue.Players.Add(loser);

        // act
        var result = new[] { game }.LostBy(new[] { winner, loser }.AsQueryable(), false, false);

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void LostBy_PlayerInLosingTeam_OnlyRedLosses_IsEmpty()
    {
        // arrange
        var player = new Player();
        var game = new Game { State = GameState.RedWon };
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(player);

        // act
        var result = new[] { game }.LostBy(new[] { player }.AsQueryable(), true, false);

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void LostBy_PlayersInBothTeams_OnlyRedAndOnlyBlueLosses_IsGame()
    {
        // arrange
        var winner = new Player();
        var loser = new Player();
        var game = new Game { State = GameState.BlueWon };
        game.Red.Players.Add(winner);
        game.Blue.Players.Add(loser);

        // act
        var result = new[] { game }.LostBy(new[] { winner, loser }.AsQueryable(), true, true);

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void WithAny_PlayersEmpty_IsEmpty()
    {
        // arrange
        var game = new Game();
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.AsQueryable().WithAny(Enumerable.Empty<Player>().AsQueryable());

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WithAny_PlayerNotInGame_IsEmpty()
    {
        // arrange
        var game = new Game();
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.AsQueryable().WithAny(new [] { new Player() }.AsQueryable());

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WithAny_AllPlayers_IsGame()
    {
        // arrange
        var winner = new Player();
        var loser = new Player();
        var game = new Game();
        game.Red.Players.Add(winner);
        game.Blue.Players.Add(loser);

        // act
        var result = new[] { game }.AsQueryable().WithAny(new[] { winner, loser }.AsQueryable());

        // assert
        CollectionAssert.AreEqual(new [] { game }, result);
    }

    [Test]
    public void WithAny_RedTeam_IsGame()
    {
        // arrange
        var player = new Player();
        var game = new Game();
        game.Red.Players.Add(player);

        // act
        var result = new[] { game }.AsQueryable().WithAny(new[] { player }.AsQueryable());

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void WithAny_BlueTeam_IsGame()
    {
        // arrange
        var player = new Player();
        var game = new Game();
        game.Blue.Players.Add(player);

        // act
        var result = new[] { game }.AsQueryable().WithAny(new[] { player }.AsQueryable());

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void WithTeam_PlayersEmpty_IsEmpty()
    {
        // arrange
        var game = new Game();
        game.Red.Players.Add(new Player());
        game.Blue.Players.Add(new Player());

        // act
        var result = new[] { game }.AsQueryable().WithTeam(Enumerable.Empty<Player>().AsQueryable());

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WithTeam_AllPlayers_IsEmpty()
    {
        // arrange
        var red = new Player();
        var blue = new Player();
        var game = new Game();
        game.Red.Players.Add(red);
        game.Blue.Players.Add(blue);

        // act
        var result = new[] { game }.AsQueryable().WithTeam(new [] { red, blue }.AsQueryable());

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void WithTeam_MoreInTeamThanAskedFor_IsGame()
    {
        // arrange
        var player = new Player();
        var game = new Game();
        game.Red.Players.Add(player);
        game.Red.Players.Add(new Player());

        // act
        var result = new[] { game }.AsQueryable().WithTeam(new[] { player }.AsQueryable());

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void PlayedOn_Futsal_WithEmptyString_IsGame()
    {
        // arrange
        var game = new Game { Stadium = "Futsal" };

        // act
        var result = new[] { game }.AsQueryable().PlayedOn(string.Empty);

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void PlayedOn_StadiumDoesNotExist_IsEmpty()
    {
        // arrange
        var game = new Game();

        // act
        var result = new[] { game }.AsQueryable().PlayedOn("Futsal");

        // assert
        CollectionAssert.IsEmpty(result);
    }

    [Test]
    public void PlayedOn_Futsal_WithFutsal_IsGame()
    {
        // arrange
        var game = new Game { Stadium = "Futsal" };

        // act
        var result = new[] { game }.AsQueryable().PlayedOn("Futsal");

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }

    [Test]
    public void PlayedOn_LowerCase_IsGame()
    {
        // arrange
        var game = new Game { Stadium = "Futsal" };

        // act
        var result = new[] { game }.AsQueryable().PlayedOn("futsal");

        // assert
        CollectionAssert.AreEqual(new[] { game }, result);
    }
}
