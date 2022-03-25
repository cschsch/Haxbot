using Haxbot;
using Haxbot.Api;
using Haxbot.Entities;
using Haxbot.Settings;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HaxbotTests;

[Parallelizable(ParallelScope.All)]
public class ApiFunctionsTests
{
    public Configuration Configuration { get; set; } = new();

    [Test]
    public void OnPlayerJoin_InvokesPlayerJoined_WithPlayer()
    {
        // arrange
        using var waitEvent = new ManualResetEventSlim(false);
        var expected = new HaxballPlayer { Name = "name" };
        var functions = new HaxballApiFunctions(default!);
        functions.PlayerJoined += (sender, args) =>
        {
            waitEvent.Set();
            Assert.AreEqual(expected, args.Player);
        };

        // act
        functions.OnPlayerJoin(expected);

        // assert
        waitEvent.Wait(1000);
        if (!waitEvent.IsSet) Assert.Fail("PlayerJoined not called within one second of calling OnPlayerJoin");
    }

    [Test]
    public void StartGame_OnlySpectators_NoPlayersInGame()
    {
        // arrange
        var games = new List<Game>();
        var context = new Mock<HaxbotContext>(Configuration).Add(games);
        var functions = new HaxballApiFunctions(context.Object);
        var players = new[] { new HaxballPlayer { Team = TeamId.Spectators } };

        // act
        functions.StartGame(players);

        // assert
        var game = games.Single();
        Assert.IsEmpty(game.Red.Players);
        Assert.IsEmpty(game.Blue.Players);
        context.Verify(ctx => ctx.SaveChanges());
    }

    [Test]
    public void StartGame_PlayerAuthInDb_UsesExistingPlayer()
    {
        // arrange
        var games = new List<Game>();
        var playersInDb = new List<Player> { new Player { Auth = "auth", Id = Guid.NewGuid() } };
        var context = new Mock<HaxbotContext>(Configuration).Add(games).Add(new List<Team>()).Add(playersInDb);
        var functions = new HaxballApiFunctions(context.Object);
        var players = new[] { new HaxballPlayer { Auth = playersInDb.Single().Auth, Team = TeamId.Red } };

        // act
        functions.StartGame(players);

        // assert
        var player = games.Single().Red.Players.Single();
        Assert.AreEqual(playersInDb.Single().Id, player.Id);
        context.Verify(ctx => ctx.SaveChanges());
    }

    [Test]
    public void StartGame_AllPlayersNew_AddedToDb()
    {
        // arrange
        var games = new List<Game>();
        var context = new Mock<HaxbotContext>(Configuration).Add(games).Add(new List<Team>()).Add(new List<Player>());
        var functions = new HaxballApiFunctions(context.Object);

        var peter = new HaxballPlayer { Name = "peter", Auth = "1", Team = TeamId.Red };
        var salomon = new HaxballPlayer { Name = "salomon", Auth = "2", Team = TeamId.Blue };

        // act
        functions.StartGame(new[] { peter, salomon });

        // assert
        var game = games.Single();
        Assert.AreEqual(GameState.Undecided, game.State);
        var peterInDatabase = game.Red.Players.Single();
        Assert.AreEqual(peter.Name, peterInDatabase.Name);
        Assert.AreEqual(peter.Auth, peterInDatabase.Auth);
        var salomonInDatabase = game.Blue.Players.Single();
        Assert.AreEqual(salomon.Name, salomonInDatabase.Name);
        Assert.AreEqual(salomon.Auth, salomonInDatabase.Auth);
        context.Verify(ctx => ctx.SaveChanges());
    }

    [Test]
    public void FinishGame_EqualScores_GameStateUndecided()
    {
        // arrange
        var games = new List<Game>();
        var context = new Mock<HaxbotContext>(Configuration).Add(games);
        var functions = new HaxballApiFunctions(context.Object);

        // act
        functions.StartGame(Array.Empty<HaxballPlayer>());
        functions.FinishGame(new());

        // assert
        var game = games.Single();
        Assert.AreEqual(GameState.Undecided, game.State);
    }

    [Test]
    public void FinishGame_RedWon_SetsRedWon()
    {
        // arrange
        var games = new List<Game>();
        var context = new Mock<HaxbotContext>(Configuration).Add(games);
        var functions = new HaxballApiFunctions(context.Object);

        // act
        functions.StartGame(Array.Empty<HaxballPlayer>());
        functions.FinishGame(new HaxballScores { Red = 1 });

        // assert
        var game = games.Single();
        Assert.AreEqual(GameState.RedWon, game.State);
    }

    [Test]
    public void FinishGame_BlueWon_SetsBlueWon()
    {
        // arrange
        var games = new List<Game>();
        var context = new Mock<HaxbotContext>(Configuration).Add(games);
        var functions = new HaxballApiFunctions(context.Object);

        // act
        functions.StartGame(Array.Empty<HaxballPlayer>());
        functions.FinishGame(new HaxballScores { Blue = 1 });

        // assert
        var game = games.Single();
        Assert.AreEqual(GameState.BlueWon, game.State);
    }

    [Test]
    public void CloseRoom_InvokesRoomClosed()
    {
        // arrange
        using var waitEvent = new ManualResetEventSlim(false);
        var context = new Mock<HaxbotContext>(Configuration);
        var functions = new HaxballApiFunctions(context.Object);
        functions.RoomClosed += (sender, args) =>
        {
            waitEvent.Set();
        };

        // act
        functions.CloseRoom();

        // assert
        waitEvent.Wait(1000);
        if (!waitEvent.IsSet) Assert.Fail("RoomClosed not called within one second of calling CloseRoom");
    }

    [Test]
    public void SetStadium_SetsStadium()
    {
        // arrange
        var games = new List<Game>();
        var context = new Mock<HaxbotContext>(Configuration).Add(games);
        var functions = new HaxballApiFunctions(context.Object);

        // act
        functions.StartGame(Array.Empty<HaxballPlayer>());
        functions.SetStadium("tollio", new());

        // assert
        var game = games.Single();
        Assert.AreEqual("tollio", game.Stadium);
    }

    [Test]
    public void Dispose_DisposesContext()
    {
        // arrange
        var context = new Mock<HaxbotContext>(Configuration);
        var functions = new HaxballApiFunctions(context.Object);

        // act
        functions.Dispose();

        // assert
        context.Verify(ctx => ctx.Dispose());
    }
}
