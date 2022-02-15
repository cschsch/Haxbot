using Haxbot;
using Haxbot.Api;
using Haxbot.Settings;
using PuppeteerSharp;

if (!Directory.Exists(Path.GetDirectoryName(HaxbotSettings.DatabasePath))) Directory.CreateDirectory(HaxbotSettings.DatabasePath);

var options = new LaunchOptions { Args = new[] { "--disable-features=WebRtcHideLocalIpsWithMdns", "--no-sandbox" }, Headless = false };
await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
var browser = await Puppeteer.LaunchAsync(options);

var token = Environment.GetCommandLineArgs().FirstOrDefault() ?? throw new ArgumentException("No token provided!");
var page = await browser.NewPageAsync();
var haxballApiFunctions = new HaxballApiFunctions(new HaxbotContext());
haxballApiFunctions.RoomClosed += async (sender, args) =>
{
    await browser.CloseAsync();
    Environment.Exit(0);
};

var api = new HaxballApi(token, page, haxballApiFunctions);
var roomLink = await api.CreateRoomAsync();
Thread.Sleep(Timeout.Infinite);