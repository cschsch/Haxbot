using Haxbot;
using Haxbot.Api;
using Haxbot.Entities;
using Haxbot.Settings;
using Microsoft.EntityFrameworkCore;
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

        var options = new LaunchOptions { Args = new[] { "--disable-features=WebRtcHideLocalIpsWithMdns", "--no-sandbox", "--no-proxy-server" }, Headless = headless };
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
        }
        catch (TaskCanceledException)
        {
        }
    }

    private static (IQueryable<Game>, IQueryable<Player>) FilterByOptions(HaxbotContext context, string[] players, bool auth, bool team, DateTime from, DateTime to, bool undecided)
    {
        var playersInDb = players.Any()
            ? auth
                ? context.Players!.ByAuth(players)
                : context.Players!.ByName(players)
            : context.Players!;
        var gamesByUndecided = context.Games!.Where(game => undecided || game.State != GameState.Undecided);
        var gamesByTime = gamesByUndecided.Between(from, to);
        var gamesByPlayers = team ? gamesByTime.WithTeam(playersInDb) : gamesByTime.WithAny(playersInDb);
        return (gamesByPlayers, playersInDb);
    }

    public void Games(string[] players, bool auth, bool team, DateTime from, DateTime to, bool undecided)
    {
        using var context = new HaxbotContext(Configuration);
        var totalGames = context.Games!.Count(game => undecided || game.State != GameState.Undecided);
        if (totalGames == 0)
        {
            Console.WriteLine("0/0 (0%)");
            return;
        }
        var (games, _) = FilterByOptions(context, players, auth, team, from, to, undecided);
        var amount = games.Count();
        Console.WriteLine($"{amount}/{totalGames} ({Math.Round(decimal.Divide(amount, totalGames) * 100, 2)}%)");
    }

    public Action<string[], bool, bool, DateTime, DateTime, bool, bool, bool> WonOrLost(GameResult result)
    {
        if (result == GameResult.Default) throw new ArgumentException("No distinction between games won and lost", nameof(result));

        return (players, auth, team, from, to, undecided, red, blue) =>
        {
            using var context = new HaxbotContext(Configuration);
            var (games, playersInDb) = FilterByOptions(context, players, auth, team, from, to, undecided);
            var filteredGames = games.Include(game => game.Red.Players).Include(game => game.Blue.Players).ToArray();
            if (filteredGames.Length == 0)
            {
                Console.WriteLine("0/0 (0%)");
                return;
            }
            var gamesByState = result == GameResult.Won ? filteredGames.WonBy(playersInDb, red, blue) : filteredGames.LostBy(playersInDb, red, blue);
            var amount = gamesByState.Count();
            Console.WriteLine($"{amount}/{filteredGames.Length} ({Math.Round(decimal.Divide(amount, filteredGames.Length) * 100, 2)}%)");
        };
    }
}
