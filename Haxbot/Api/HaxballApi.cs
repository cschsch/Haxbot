using Haxbot.Extensions;
using Haxbot.Settings;
using PuppeteerSharp;

namespace Haxbot.Api;

public class HaxballApi : IDisposable
{
    public string? Token { get; }
    public Page Page { get; }
    public IHaxballApiFunctions ApiFunctions { get; }
    public IPartyManager PartyManager { get; }
    private Configuration Configuration { get; }

    public HaxballApi(IHaxballApiFunctions apiFunctions, IPartyManager partyManager, Configuration configuration, Page page, string? token)
    {
        Token = token;
        Page = page;
        ApiFunctions = apiFunctions;
        PartyManager = partyManager;
        Configuration = configuration;
    }

    public async Task<string> CreateRoomAsync(int timeOut = 0)
    {
        await Page.GoToAsync(Configuration.HaxballHeadlessUrl, WaitUntilNavigation.Networkidle2);
        await ExposeFunctions();

        await Page.EvaluateFunctionAsync(@"(roomConfig, token, admins) => {
    roomConfig.token = token;
    room = HBInit(roomConfig);
    room.setTimeLimit(roomConfig.timeLimit);
    room.stadium = 'Classic';
    idAuths = [];
    room.onPlayerJoin = async function (player) {
        idAuths.push([player.id, player.auth]);
        await playerJoined(player);
        const newTeamSetup = await nextTeamSetup(room.getPlayerList());
        for (const player of newTeamSetup) { room.setPlayerTeam(player.id, player.team); }
        if (!admins.includes(player.auth)) return;
        room.setPlayerAdmin(player.id, true);
    };
    room.onGameStart = async function (byPlayer) {
        const players = room.getPlayerList();
        const couldStartGame = await startGame(room.stadium, players, idAuths);
        if (couldStartGame) 
        {
            room.startRecording();
            return;
        }
        room.sendChat(`Failed to save game to database!`);
    };
    room.onTeamVictory = async function (scores) {
        const couldFinishGame = await finishGame(scores);
        if (couldFinishGame) return;
        room.sendChat(`Failed to save results to database!`);
    };
    room.onPlayerLeave = async function (player) {
        const players = room.getPlayerList();
        if (players.length === 0 || (players.length === 1 && players[0].name === roomConfig.playerName)) {
            await closeRoom();
        }
    };
    room.onStadiumChange = async function (stadium, player) {
        room.stadium = stadium;
    };
    room.onPlayerChat = async function (player, message) {
        const answer = await handleCommand(player, message);
        room.sendChat(answer);
    };
    room.onGameStop = async function (byPlayer) {
        const replay = room.stopRecording();
        const base64 = btoa(String.fromCharCode.apply(null, replay));
        await saveReplay(base64);
        await finishGame({ red: 0, blue: 0 });
        const newTeamSetup = await nextTeamSetup(room.getPlayerList());
        for (const player of newTeamSetup) { room.setPlayerTeam(player.id, player.team); }
    }
}", Configuration.RoomConfiguration, Token, Configuration.RoomAdmins);

        return await Page
            .Frames
            .Single(frame => frame != Page.MainFrame)
            .WaitForSelectorAsync("#roomlink a", new WaitForSelectorOptions { Timeout = timeOut })
            .Bind(handle => handle.GetPropertyAsync("href"))
            .Map(handle => handle.RemoteObject.Value.ToString());
    }

    private async Task ExposeFunctions()
    {
        var exposePlayerJoined = Page.ExposeFunctionAsync<HaxballPlayer, object>("playerJoined", player => { ApiFunctions.OnPlayerJoin(player); return default!; });
        var exposeStartGame = Page.ExposeFunctionAsync<string, HaxballPlayer[], string[][], bool>("startGame", (stadium, players, idAuths) => ApiFunctions.StartGame(stadium, players.Select(player => player.EnrichAuth(idAuths)).ToArray()));
        var exposeFinishGame = Page.ExposeFunctionAsync<HaxballScores, bool>("finishGame", ApiFunctions.FinishGame);
        var exposeCloseRoom = Page.ExposeFunctionAsync("closeRoom", ApiFunctions.CloseRoom);
        var exposeHandleCommand = Page.ExposeFunctionAsync<HaxballPlayer, string, string>("handleCommand", ApiFunctions.HandleCommand);
        var exposeSaveReplay = Page.ExposeFunctionAsync<string, object>("saveReplay", base64 => { ApiFunctions.SaveReplay(base64); return default!; });
        var exposeNextTeamSetup = Page.ExposeFunctionAsync<HaxballPlayer[], HaxballPlayer[]>("nextTeamSetup", NextTeamSetup);
        await Task.WhenAll(exposePlayerJoined, exposeStartGame, exposeFinishGame, exposeHandleCommand, exposeSaveReplay, exposeNextTeamSetup);
    }

    private HaxballPlayer[] NextTeamSetup(HaxballPlayer[] players) =>
        Configuration.RoomConfiguration.PartyManagement switch
        {
            PartyManagement.None => PartyManager.None(players),
            PartyManagement.Shuffle => PartyManager.Shuffle(players),
            PartyManagement.RoundRobin => PartyManager.RoundRobin(players),
            _ => Array.Empty<HaxballPlayer>()
        };

    public void Dispose()
    {
        Page.CloseAsync().Wait();
    }
}