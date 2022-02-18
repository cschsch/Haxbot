using Haxbot.Settings;
using PuppeteerSharp;

namespace Haxbot.Api;

public class HaxballApi
{
    public string? Token { get; }
    public Page Page { get; }
    public IHaxballApiFunctions ApiFunctions { get; }
    private Configuration Configuration { get; }

    public HaxballApi(IHaxballApiFunctions apiFunctions, Configuration configuration, Page page, string? token)
    {
        Token = token;
        Page = page;
        ApiFunctions = apiFunctions;
        Configuration = configuration;
    }

    public async Task<string> CreateRoomAsync()
    {
        await Page.GoToAsync(Configuration.HaxballHeadlessUrl, WaitUntilNavigation.Networkidle2);
        await ExposeFunctions();

        await Page.EvaluateFunctionAsync(@"(roomConfig, token, admins) => {
    roomConfig.token = token;
    room = HBInit(roomConfig);
    idAuths = [];
    room.onPlayerJoin = async function (player) {
        idAuths.push([player.id, player.auth]);
        await playerJoined(player);
        if (!admins.includes(player.auth)) return;
        room.setPlayerAdmin(player.id, true);
    };
    room.onGameStart = async function (byPlayer) {
        const players = room.getPlayerList();
        const couldStartGame = await startGame(players, idAuths);
        if (couldStartGame) return;
        room.sendChat(`Failed to save game to database!`);
    };
    room.onTeamVictory = async function (scores) {
        const couldFinishGame = await finishGame(scores);
        if (couldFinishGame) return;
        room.sendChat(`Failed to save results to database!`);
    };
    room.onPlayerLeave = async function (player) {
        if (room.getPlayerList().length === 0) await closeRoom();
    };
    room.onPlayerChat = async function (player, message) {
        const answer = await handleCommand(player, message);
        room.sendChat(answer);
    };
}", Configuration.RoomConfiguration, Token, Configuration.RoomAdmins);

        return await Page
            .Frames
            .Single(frame => frame != Page.MainFrame)
            .WaitForSelectorAsync("#roomlink a")
            .Bind(handle => handle.GetPropertyAsync("href"))
            .Map(handle => handle.RemoteObject.Value.ToString());
    }

    private async Task ExposeFunctions()
    {
        var exposePlayerJoined = Page.ExposeFunctionAsync<HaxballPlayer, object>("playerJoined", player => { ApiFunctions.OnPlayerJoin(player); return default!; });
        var exposeStartGame = Page.ExposeFunctionAsync<HaxballPlayer[], string[][], bool>("startGame", (players, idAuths) => ApiFunctions.StartGame(players.Select(player => player.EnrichAuth(idAuths)).ToArray()));
        var exposeFinishGame = Page.ExposeFunctionAsync<HaxballScores, bool>("finishGame", ApiFunctions.FinishGame);
        var exposeCloseRoom = Page.ExposeFunctionAsync("closeRoom", ApiFunctions.CloseRoom);
        var exposeHandleCommand = Page.ExposeFunctionAsync<HaxballPlayer, string, string>("handleCommand", ApiFunctions.HandleCommand);
        await Task.WhenAll(exposePlayerJoined, exposeStartGame, exposeFinishGame, exposeHandleCommand);
    }
}