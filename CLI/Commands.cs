using CLI.Extensions;
using Haxbot.Settings;
using System.CommandLine;
using System.CommandLine.Binding;

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
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.TimeLimit)}", new [] { "--time-limit" } }
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

    private Command GetStandardCommand(Configuration configuration)
    {
        var standardCommand = new Command("standard", "Execute standard overview commands as specified in configuration. Commands will be formatted with current date.");
        standardCommand.SetHandler(() =>
        {
            foreach (var command in configuration.StandardOverviewCommands
                .Where(command => command.Contains("overview"))
                .Select(command => string.Format(command, new [] { DateTime.Today.ToShortDateString() })))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(command + Environment.NewLine);
                Console.ResetColor();
                Execute(command.Split(" "));
            }
        });
        return standardCommand;
    }

    private Command GetWonOrLostCommand(HaxbotApp app, BinderBase<QueryFilter> filterBinder, GameResult result)
    {
        var verb = result switch
        {
            GameResult.Won => "won",
            GameResult.Lost => "lost",
            _ => throw new ArgumentException("Result has to be Won or Lost", nameof(result))
        };
        var command = new Command(verb, $"Count the amount of games {verb} against the total amount of filtered games.");
        var redOption = new Option<bool>(new[] { "--red", "-r" }, () => false, "Constrain to games won by the red team.");
        var blueOption = new Option<bool>(new[] { "--blue", "-b" }, () => false, "Constrain to games won by the blue team.");
        command.AddOptions(redOption, blueOption);
        command.SetHandler((QueryFilter filter, bool redWon, bool blueWon) => app.WonOrLost(result)(filter, redWon, blueWon), filterBinder, redOption, blueOption);
        return command;
    }
}