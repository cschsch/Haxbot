using CLI.Extensions;
using Haxbot.Settings;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace CLI;

public partial class Program
{
    private IDictionary<string, string[]> MainSwitches { get; } = new Dictionary<string, string[]>
    {
        { nameof(Configuration.DatabasePath), new [] { "--database", "-d" } },
        { nameof(Configuration.ConnectionStringTemplate), new [] { "--connection-string-template", "-c" } }
    };

    private IDictionary<string, string[]> CreateRoomSwitches { get; } = new Dictionary<string, string[]>
    {
        { nameof(Configuration.HaxballHeadlessUrl), new [] { "--url", "-u" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.RoomName)}", new [] { "--room-name", "-r" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Password)}", new [] { "--password", "-p" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Public)}", new [] { "--public" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.NoPlayer)}", new [] { "--no-player" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.TimeLimit)}", new [] { "--time-limit", "-t" } }
    };

    private RootCommand GetRootCommand(Configuration configuration)
    {
        var rootCommand = new RootCommand("Create a haxball room and output its url or query for statistics gathered in such rooms.");
        var pathOption = new Option<string>(MainSwitches[nameof(Configuration.DatabasePath)], () => configuration.DatabasePath, "Path pointing to database to be used. May not exist yet.");
        var connectionStringOption = new Option<string>(MainSwitches[nameof(Configuration.ConnectionStringTemplate)], () => configuration.ConnectionStringTemplate, "string.Format to be used to connect to the database. Can be used to inject arguments. DatabasePath will be inserted at {0}.");
        rootCommand.AddGlobalOptions(pathOption, connectionStringOption);
        return rootCommand;
    }

    private Command GetCreateCommand(Configuration configuration, HaxbotApp app)
    {
        var createCommand = new Command("create", "Create a new room and output its url.")
        {
            new Option<string>(CreateRoomSwitches[nameof(Configuration.HaxballHeadlessUrl)], () => configuration.HaxballHeadlessUrl, "URL pointing to Haxball headless API"),
            new Option<string>(CreateRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.RoomName)}"], () => configuration.RoomConfiguration.RoomName, "Name of room to be created"),
            new Option<string>(CreateRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Password)}"], () => configuration.RoomConfiguration.Password ?? "<none>", "Password to be assigned to room"),
            new Option<bool>(CreateRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Public)}"], () => configuration.RoomConfiguration.Public, "Whether to make the room public"),
            new Option<bool>(CreateRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.NoPlayer)}"], () => configuration.RoomConfiguration.NoPlayer, "Set to false if you wish the bot to join. Required for command handling."),
            new Option<int>(CreateRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.TimeLimit)}"], () => configuration.RoomConfiguration.TimeLimit, "Set time limit of games played")
        };
        var tokenOption = new Option<string?>(new[] { "--token", "-t" }, "Haxball API token. Required if headless. Obtain here: https://www.haxball.com/headlesstoken");
        var headlessOption = new Option<bool>("--headless", () => false, "Whether to run the browser headless or not.");
        createCommand.AddOptions(tokenOption, headlessOption);
        createCommand.SetHandler((string? token, bool headless) => app.CreateRoom(token, headless), tokenOption, headlessOption);
        return createCommand;
    }

    private Command GetOverviewCommand(HaxbotApp app, BinderBase<QueryFilter> filterBinder)
    {
        var overviewCommand = new Command("overview", "Prints an overview for each of the given options.");
        var resultGroupingsNames = StatsPrinter.ResultGroupings.Select(grouping => grouping.ToString());
        var groupingsArgument = new Argument<Grouping[]>("Groupings", () => new[] { Grouping.Player },
        $@"Groupings to apply before counting results

Possible Groupings -> {string.Join(", ", Enum.GetNames(typeof(Grouping)).Except(resultGroupingsNames))}

Last value needs to be a result grouping, of which there can only be one.

Result Groupings -> {string.Join(", ", resultGroupingsNames)}

");
        var statsPrinterBinder = new StatsPrinterBinder(groupingsArgument);
        overviewCommand.AddArgument(groupingsArgument);
        overviewCommand.SetHandler((QueryFilter filter, StatsPrinter collector) => app.Overview(filter, collector), filterBinder, statsPrinterBinder);
        return overviewCommand;
    }

    Command GetWonCommand(HaxbotApp app, BinderBase<QueryFilter> filterBinder)
    {
        var wonCommand = new Command("won", "Count the amount of games won against the total amount of filtered games.");
        var redWonOption = new Option<bool>(new[] { "--red", "-r" }, () => false, "Constrain to games won by the red team.");
        var blueWonOption = new Option<bool>(new[] { "--blue", "-b" }, () => false, "Constrain to games won by the blue team.");
        wonCommand.AddOptions(redWonOption, blueWonOption);
        wonCommand.SetHandler((QueryFilter filter, bool redWon, bool blueWon) => app.WonOrLost(GameResult.Won)(filter, redWon, blueWon), filterBinder, redWonOption, blueWonOption);
        return wonCommand;
    }

    Command GetLostCommand(HaxbotApp app, BinderBase<QueryFilter> filterBinder)
    {
        var lostCommand = new Command("lost", "Count the amount of games lost against the total amount of filtered games.");
        var redLostOption = new Option<bool>(new[] { "--red", "-r" }, () => false, "Constrain to games lost by the red team.");
        var blueLostOption = new Option<bool>(new[] { "--blue", "-b" }, () => false, "Constrain to games lost by the blue team.");
        lostCommand.AddOptions(redLostOption, blueLostOption);
        lostCommand.SetHandler((QueryFilter filter, bool redLost, bool blueLost, ParseResult parseResult) => app.WonOrLost(GameResult.Lost)(filter, redLost, blueLost), filterBinder, redLostOption, blueLostOption);
        return lostCommand;
    }
}