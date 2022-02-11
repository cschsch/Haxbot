using Haxbot.Settings;
using PuppeteerSharp;

namespace Haxbot.Api;

public class HaxballApi
{
    public string Token { get; }
    public Page Page { get; }
    public HaxballApiFunctions HaxballApiFunctions { get; }

    public HaxballApi(string token, Page page, HaxballApiFunctions haxballApiFunctions)
    {
        Token = token;
        Page = page;
        HaxballApiFunctions = haxballApiFunctions;
    }

    public async Task<string> CreateRoomAsync()
    {
        await Page.GoToAsync(HaxbotSettings.HaxballHeadlessUrl, WaitUntilNavigation.Networkidle2);
        await ExposeFunctions();

        await Page.EvaluateFunctionAsync(@"(roomConfig, token, admins) => {
    roomConfig.token = token;
    room = HBInit(roomConfig);
    room.onPlayerJoin = function (player) {      
        if (admins.includes(player.auth))
            room.setPlayerAdmin(player.id, true);
    };
    room.onGameStart = function (byPlayer) {
        const players = getPlayerList();
        players.forEach(player => addPlayer(player));
    };
    room.onTeamVictory = function (scores) {
        addFinishedGame(scores);
    }
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
        var exposeAddPlayer = Page.ExposeFunctionAsync<HaxballPlayer, bool>("addPlayer", player => HaxballApiFunctions.AddPlayer(player));
        var exposeAddFinishedGame = Page.ExposeFunctionAsync<HaxballScores, bool>("addFinishedGame", scores => HaxballApiFunctions.AddFinishedGame(scores));
        await Task.WhenAll(exposeAddPlayer, exposeAddFinishedGame);
    }
}