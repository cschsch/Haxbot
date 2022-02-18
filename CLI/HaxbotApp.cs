using Haxbot;
using Haxbot.Api;
using Haxbot.Settings;
using PuppeteerSharp;

namespace CLI;

public class HaxbotApp
{
    public Configuration Configuration { get; }

    public HaxbotApp(Configuration configuration)
    {
        Configuration = configuration;
    }

    public void Init()
    {
        if (!Directory.Exists(Path.GetDirectoryName(Configuration.DatabasePath))) Directory.CreateDirectory(Configuration.DatabasePath);
    }

    public async Task CreateRoom(string? token, bool headless)
    {
        if (string.IsNullOrWhiteSpace(token) && headless) throw new ArgumentException("token cannot be empty in headless mode!");

        var cancellationTokenSource = new CancellationTokenSource();

        var options = new LaunchOptions { Args = new[] { "--disable-features=WebRtcHideLocalIpsWithMdns", "--no-sandbox" }, Headless = headless };
        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        var browser = await Puppeteer.LaunchAsync(options);
        var page = await browser.NewPageAsync();

        var haxballApiFunctions = new HaxballApiFunctions(new HaxbotContext(Configuration));
        haxballApiFunctions.RoomClosed += async (sender, args) =>
        {
            await browser.DisposeAsync();
            cancellationTokenSource.Cancel();
        };

        var api = new HaxballApi(haxballApiFunctions, Configuration, page, token);
        var roomLink = await api.CreateRoomAsync();
        Console.WriteLine(roomLink);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        } catch (TaskCanceledException)
        {
        }
    }
}
