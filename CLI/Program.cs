using CLI.Commands;
using Haxbot.Settings;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace CLI;

public class Program
{
    private static Configuration GetConfiguration(string[] args) => new ConfigurationBuilder()
        .AddJsonFile("haxbotconfig.json")
        .AddCommandLine(args,
            CommandSwitches.RootSwitches
            .Concat(CommandSwitches.CreateRoomSwitches)
            .SelectMany(nameSwitches => nameSwitches.Value.Select(@switch => new KeyValuePair<string, string>(@switch, nameSwitches.Key)))
            .ToDictionary(kv => kv.Key, kv => kv.Value))
        .Build()
        .Get<Configuration>();

    public static void Main(string[] args)
    {
        var configuration = GetConfiguration(args);

        var app = new HaxbotApp(configuration);

        var rootCommand = CommandFactories.GetRootCommand(configuration);
        var createCommand = CommandFactories.GetCreateCommand(configuration, app);

        var mainQueryCommand = new QueryCommand();
        var queryCommand = mainQueryCommand.GetCommand("query", "Query the database. You can append options to this command to pre-filter the set of games queried in subsequent commands.", true);

        var mainGamesCommand = new QueryCommand();
        var gamesCommand = mainGamesCommand.GetCommand("games", "Query for games. Returns amount of games played against the total amount of games.");
        gamesCommand.SetHandler((QueryFilter preFilter, QueryFilter filter) => app.Games(preFilter, filter), mainQueryCommand, mainGamesCommand);

        var overviewCommand = CommandFactories.GetOverviewCommand(app, mainQueryCommand);
        var standardCommand = CommandFactories.GetStandardCommand(configuration, Main);

        var wonCommand = CommandFactories.GetWonOrLostCommand(app, mainQueryCommand, GameResult.Won);
        var lostCommand = CommandFactories.GetWonOrLostCommand(app, mainQueryCommand, GameResult.Lost);

        var commandHierarchy = new CommandNode(rootCommand)
        {
            createCommand,
            new(queryCommand)
            {
                new(gamesCommand)
                {
                    wonCommand,
                    lostCommand,
                    new(overviewCommand)
                    {
                        standardCommand
                    }
                }
            }
        };
        commandHierarchy.InitializeCommand();

        app.Init();
        rootCommand.Invoke(args);
    }
}