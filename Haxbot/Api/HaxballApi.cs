using Haxbot.Settings;
using PuppeteerSharp;

namespace Haxbot.Api;

public class HaxballApi
{
    public string Token { get; }
    public Page Page { get; }
    public HaxballApiFunctions ApiFunctions { get; }

    public HaxballApi(string token, Page page, HaxballApiFunctions apiFunctions)
    {
        Token = token;
        Page = page;
        ApiFunctions = apiFunctions;
    }

    public async Task<string> CreateRoomAsync()
    {
        await Page.GoToAsync(HaxbotSettings.HaxballHeadlessUrl, WaitUntilNavigation.Networkidle2);
        await ExposeFunctions();

        await Page.EvaluateFunctionAsync(@"(roomConfig, token, admins) => {
    roomConfig.token = token;
    room = HBInit(roomConfig);
    room.onPlayerJoin = function (player) {      
        if (!admins.includes(player.auth)) return;
        room.setPlayerAdmin(player.id, true);
    };
    room.onGameStart = function (byPlayer) {
        const players = getPlayerList();
        const couldStartGame = startGame(players);
        if (couldStartGame) return;
        room.sendChat(`Failed to save game to database!`);
    };
    room.onTeamVictory = function (scores) {
        const couldFinishGame = finishGame(scores);
        if (couldFinishGame) return;
        room.sendChat(`Failed to save results to database!`);
    };
    room.onPlayerChat = function (player, message) {
        const answer = handleCommand(player, message);
        room.sendChat(answer);
    };
}", HaxbotSettings.RoomConfiguration, Token, HaxbotSettings.Admins);

        return await Page
            .Frames
            .Single(frame => frame != Page.MainFrame)
            .WaitForSelectorAsync("#roomlink a")
            .Bind(handle => handle.GetPropertyAsync("href"))
            .Map(handle => handle.RemoteObject.Value.ToString());
    }

    private async Task ExposeFunctions()
    {
        var exposeStartGame = Page.ExposeFunctionAsync<HaxballPlayer[], bool>("startGame", ApiFunctions.StartGame);
        var exposeFinishGame = Page.ExposeFunctionAsync<HaxballScores, bool>("finishGame", ApiFunctions.FinishGame);
        var exposeHandleCommand = Page.ExposeFunctionAsync<HaxballPlayer, string, string>("handleCommand", ApiFunctions.HandleCommand);
        await Task.WhenAll(exposeStartGame, exposeFinishGame, exposeHandleCommand);
    }
}