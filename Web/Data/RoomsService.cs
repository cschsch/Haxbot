using Haxbot;
using Haxbot.Api;
using Haxbot.Settings;
using PuppeteerSharp;

namespace Web.Data;

public class RoomsService
{
    public HashSet<Room> Rooms { get; } = new();
    private Browser? Browser { get; set; }

    private async Task InitBrowser(Configuration configuration)
    {
        var options = new LaunchOptions { Args = configuration.ChromiumArgs, Headless = true };
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        Browser = await Puppeteer.LaunchAsync(options);
    }

    public async Task<string> CreateRoom(Configuration configuration, string token)
    {
        if (Browser is null) await InitBrowser(configuration);

        var apiFunctions = new HaxballApiFunctions(new HaxbotContext(configuration));
        var api = new HaxballApi(apiFunctions, configuration, await Browser!.NewPageAsync(), token);

        var urlTask = api.CreateRoomAsync(5000);
        string url;
        try
        {
            url = await urlTask;
        } catch (Exception)
        {
            api.Dispose();
            throw urlTask.Exception!;
        }

        if (Rooms.Any(room => room.Url == url))
        {
            api.Dispose();
            return url;
        }

        apiFunctions.RoomClosed += (sender, args) =>
        {
            var room = Rooms.Single(room => room.Url == url);
            lock (Rooms) Rooms.Remove(room);
            api.Dispose();
        };

        var room = new Room(url, configuration.RoomAdmins, configuration.RoomConfiguration);
        lock (Rooms) Rooms.Add(room);
        return url;
    }
}