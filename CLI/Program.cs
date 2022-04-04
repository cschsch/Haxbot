﻿using CLI;
using CLI.Extensions;
using Haxbot.Settings;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace CLI;

public partial class Program
{
    private Configuration GetConfiguration(string[] args) => new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddCommandLine(args,
            MainSwitches
            .Concat(CreateRoomSwitches)
            .SelectMany(nameSwitches => nameSwitches.Value.Select(@switch => new KeyValuePair<string, string>(@switch, nameSwitches.Key)))
            .ToDictionary(kv => kv.Key, kv => kv.Value))
        .Build()
        .Get<Configuration>();

    static void Main(string[] args)
    {
        var program = new Program();
        program.Execute(args);
    }

    public void Execute(string[] args)
    {
        var configuration = GetConfiguration(args);

        var app = new HaxbotApp(configuration);

        var rootCommand = GetRootCommand(configuration);
        var createCommand = GetCreateCommand(configuration, app);

        var mainQueryCommand = new QueryCommand();
        var queryCommand = mainQueryCommand.GetCommand("query", "Query the database. You can append options to this command to pre-filter the set of games queried in subsequent commands.");

        var mainGamesCommand = new QueryCommand();
        var gamesCommand = mainGamesCommand.GetCommand("games", "Query for games. Returns amount of games played against the total amount of games.", true);
        gamesCommand.SetHandler((QueryFilter preFilter, QueryFilter filter) => app.Games(preFilter, filter), mainQueryCommand, mainGamesCommand);

        var overviewCommand = GetOverviewCommand(app, mainGamesCommand);
        var standardCommand = GetStandardCommand(configuration);

        var wonCommand = GetWonOrLostCommand(app, mainGamesCommand, GameResult.Won);
        var lostCommand = GetWonOrLostCommand(app, mainGamesCommand, GameResult.Lost);

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