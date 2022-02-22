using Haxbot;
using Haxbot.Api;
using Haxbot.Entities;
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
        using var waitEvent = new ManualResetEventSlim(false);
        haxballApiFunctions.PlayerJoined += (sender, args) =>
        {
            waitEvent.Set();
        };

        var api = new HaxballApi(haxballApiFunctions, Configuration, page, token);
        var roomLink = await api.CreateRoomAsync();
        Console.WriteLine(roomLink);

        var playerHasEntered = waitEvent.Wait(30000);
        if (!playerHasEntered) cancellationTokenSource.Cancel();

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        } catch (TaskCanceledException)
        {
        }
    }

    public void Names(string auth)
    {
        using var context = new HaxbotContext(Configuration);
        var names = context.Players!.Where(player => player.Auth == auth).Select(player => player.Name);
        var output = string.Join(Environment.NewLine, names);
        Console.WriteLine(output);
    }

    private static (IQueryable<Game>, IQueryable<Player>) FilterByOptions(HaxbotContext context, string[] players, bool auth, bool team, DateTime from, DateTime to)
    {
        var playersInDb = players.Any()
            ? auth 
                ? context.Players!.ByAuth(players) 
                : context.Players!.ByName(players)
            : context.Players!;
        var gamesByTime = context.Games!.Between(from, to);
        var gamesByPlayers = team ? gamesByTime.WithTeam(playersInDb) : gamesByTime.WithAny(playersInDb);
        return (gamesByPlayers, playersInDb);
    }

    public void Games(string[] players, bool auth, bool team, DateTime from, DateTime to)
    {
        using var context = new HaxbotContext(Configuration);
        var totalGames = context.Games!.Count();
        var (games, _) = FilterByOptions(context, players, auth, team, from, to);
        var amount = games.Count();
        Console.WriteLine($"{amount}/{totalGames} ({Math.Round(decimal.Divide(amount, totalGames) * 100, 2)}%)");
    }

    public Action<string[], bool, bool, DateTime, DateTime, bool, bool> WonOrLost(GameResult result)
    {
        if (result == GameResult.Default) throw new ArgumentException("No distinction between games won and lost", nameof(result));

        return (players, auth, team, from, to, red, blue) =>
        {
            using var context = new HaxbotContext(Configuration);
            var totalGames = context.Games!.Count();
            var (games, playersInDb) = FilterByOptions(context, players, auth, team, from, to);
            var gamesByState = result == GameResult.Won ? games.WonBy(playersInDb, red, blue) : games.LostBy(playersInDb, red, blue);
            var amount = gamesByState.Count();
            Console.WriteLine($"{amount}/{totalGames} ({Math.Round(decimal.Divide(amount, totalGames) * 100, 2)}%)");
        };
    }
}
