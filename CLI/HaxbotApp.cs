using Haxbot;
using Haxbot.Api;
using Haxbot.Entities;
using Haxbot.Settings;
using Haxbot.Stats;
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
        if (string.IsNullOrWhiteSpace(token) && headless) throw new ArgumentException("Token cannot be empty in headless mode!");

        var cancellationTokenSource = new CancellationTokenSource();

        var options = new LaunchOptions { Args = Configuration.ChromiumArgs, Headless = headless };
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

    private static (IQueryable<Game>, IQueryable<Player>) FilterByOptions(HaxbotContext context, QueryFilter filter)
        => FilterByOptions(context, filter, context.Games!);

    private static (IQueryable<Game>, IQueryable<Player>) FilterByOptions(HaxbotContext context, QueryFilter filter, IQueryable<Game> games)
    {
        Team GetTeam(IQueryable<Player> players)
        {
            var teamPlayers = players.ToArray();
            var team = context.Teams!.Include(team => team.Players).AsEnumerable().SingleOrDefault(team => team.Players.Count == teamPlayers.Length && teamPlayers.All(player => team.Players.Contains(player)));
            return team ?? new Team();
        }

        var playersInDb = filter.Players.Any()
            ? filter.Auth
                ? context.Players!.ByAuth(filter.Players)
                : context.Players!.ByName(filter.Players)
            : context.Players!;
        var gamesByUndecided = games.Where(game => filter.Undecided || game.State != GameState.Undecided);
        var gamesByTime = gamesByUndecided.Between(filter.From, filter.To);
        var gamesByPlayers = filter.Team ? gamesByTime.WithTeam(GetTeam(playersInDb)) : gamesByTime.WithAny(playersInDb);
        var gamesByStadium = gamesByPlayers.PlayedOn(filter.Stadium);
        return (gamesByStadium, playersInDb);
    }

    public void Games(QueryFilter preFilter, QueryFilter filter)
    {
        using var context = new HaxbotContext(Configuration);
        var (preFiltered, _) = FilterByOptions(context, preFilter);
        var totalGames = preFiltered.Count();
        if (totalGames == 0)
        {
            Console.WriteLine("0/0 (0%)");
            return;
        }
        var (games, _) = FilterByOptions(context, filter, preFiltered);
        var amount = games.Count();
        Console.WriteLine($"{amount}/{totalGames} ({Math.Round(decimal.Divide(amount, totalGames) * 100, 2)}%)");
    }

    public Action<QueryFilter, bool, bool> WonOrLost(GameResult result)
    {
        if (result == GameResult.Default) throw new ArgumentException("No distinction between games won and lost!", nameof(result));

        return (filter, red, blue) =>
        {
            using var context = new HaxbotContext(Configuration);
            var (games, players) = FilterByOptions(context, filter);
            var filteredGames = games.Include(game => game.Red.Players).Include(game => game.Blue.Players).ToArray();
            if (filteredGames.Length == 0)
            {
                Console.WriteLine("0/0 (0%)");
                return;
            }
            var gamesByState = result == GameResult.Won ? filteredGames.WonBy(players, red, blue) : filteredGames.LostBy(players, red, blue);
            var amount = gamesByState.Count();
            Console.WriteLine($"{amount}/{filteredGames.Length} ({Math.Round(decimal.Divide(amount, filteredGames.Length) * 100, 2)}%)");
        };
    }

    public void Overview(QueryFilter filter, StatsPrinter collector)
    {
        using var context = new HaxbotContext(Configuration);
        var (dbGames, dbPlayers) = FilterByOptions(context, filter);
        var games = dbGames.Include(game => game.Red.Players).Include(game => game.Blue.Players).ToArray();
        var players = dbPlayers.ToArray();

        collector.PrintStats(games, players);
    }
}