using CLI;
using CLI.Extensions;
using Haxbot.Settings;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Parsing;

#region Configuration
var mainSwitches = new Dictionary<string, string[]>
{
    { nameof(Configuration.DatabasePath), new [] { "--database", "-d" } },
    { nameof(Configuration.ConnectionStringTemplate), new [] { "--connection-string-template", "-c" } }
};
var createRoomSwitches = new Dictionary<string, string[]>
{
    { nameof(Configuration.HaxballHeadlessUrl), new [] { "--url", "-u" } },
    { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.RoomName)}", new [] { "--room-name", "-r" } },
    { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Password)}", new [] { "--password", "-p" } },
    { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Public)}", new [] { "--public" } },
    { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.NoPlayer)}", new [] { "--no-player" } }
};

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(Environment.GetCommandLineArgs(),
        mainSwitches
        .Concat(createRoomSwitches)
        .SelectMany(nameSwitches => nameSwitches.Value.Select(@switch => new KeyValuePair<string, string>(@switch, nameSwitches.Key)))
        .ToDictionary(kv => kv.Key, kv => kv.Value))
    .Build()
    .Get<Configuration>();
#endregion

var app = new HaxbotApp(configuration);

var rootCommand = new RootCommand("Create a haxball room and output its url or query for statistics gathered in such rooms.");
var pathOption = new Option<string>(mainSwitches[nameof(Configuration.DatabasePath)], () => configuration.DatabasePath, "Path pointing to database to be used. May not exist yet.");
var connectionStringOption = new Option<string>(mainSwitches[nameof(Configuration.ConnectionStringTemplate)], () => configuration.ConnectionStringTemplate, "string.Format to be used to connect to the database. Can be used to inject arguments. DatabasePath will be inserted at {0}.");
rootCommand.AddGlobalOptions(pathOption, connectionStringOption);

#region Create
var createCommand = new Command("create", "Create a new room and output its url.")
{
    new Option<string>(createRoomSwitches[nameof(Configuration.HaxballHeadlessUrl)], () => configuration.HaxballHeadlessUrl, "URL pointing to Haxball headless API"),
    new Option<string>(createRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.RoomName)}"], () => configuration.RoomConfiguration.RoomName, "Name of room to be created"),
    new Option<string>(createRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Password)}"], () => configuration.RoomConfiguration.Password ?? "<none>", "Password to be assigned to room"),
    new Option<bool>(createRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Public)}"], () => configuration.RoomConfiguration.Public, "Whether to make the room public"),
    new Option<bool>(createRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.NoPlayer)}"], () => configuration.RoomConfiguration.NoPlayer, "Set to false if you wish the bot to join. Required for command handling.")
};
var tokenOption = new Option<string?>(new[] { "--token", "-t" }, "Haxball API token. Required if headless. Obtain here: https://www.haxball.com/headlesstoken");
var headlessOption = new Option<bool>("--headless", () => false, "Whether to run the browser headless or not.");
createCommand.AddOptions(tokenOption, headlessOption);
createCommand.SetHandler((string? token, bool headless) => app.CreateRoom(token, headless), tokenOption, headlessOption);
#endregion

#region Query
var queryCommand = new Command("query", "Query the database. You can append options to this command to pre-filter the set of games queried in subsequent commands.");
var preFilterPlayersOption = new Option<string[]>(new[] { "--players", "-p" }, "Look for games where any of these players participated.") { Arity = ArgumentArity.ZeroOrMore, AllowMultipleArgumentsPerToken = true };
var preFilterAuthOption = new Option<bool>(new[] { "--auth", "-a" }, () => false, "Look for public Auth instead of name. See https://www.haxball.com/playerauth");
var preFilterTeamOption = new Option<bool>(new[] { "--team", "-t" }, () => false, "Look for games where all of the given players were in one team.");
var preFilterFromOption = new Option<DateTime>("--from", () => DateTime.MinValue, "Only count games played since the give time.");
var preFilterToOption = new Option<DateTime>("--to", () => DateTime.MaxValue, "Only count games played until the given time.");
var preFilterUndecidedOption = new Option<bool>(new[] { "--undecided", "-u" }, () => false, "Include undecided games.");
var preFilterStadiumOption = new Option<string>(new[] { "--stadium", "-s" }, () => string.Empty, "Look for games played on given stadium. Returns non-exact case-sensitive matches.");
queryCommand.AddOptions(preFilterPlayersOption, preFilterAuthOption, preFilterTeamOption, preFilterFromOption, preFilterToOption, preFilterUndecidedOption, preFilterStadiumOption);

QueryFilter GetPreFilter(ParseResult parseResult)
{
    return new QueryFilter
    {
        Players = parseResult.GetValueForOption(preFilterPlayersOption),
        Auth = parseResult.GetValueForOption(preFilterAuthOption),
        Team = parseResult.GetValueForOption(preFilterTeamOption),
        From = parseResult.GetValueForOption(preFilterFromOption),
        To = parseResult.GetValueForOption(preFilterToOption),
        Undecided = parseResult.GetValueForOption(preFilterUndecidedOption),
        Stadium = parseResult.GetValueForOption<string>(preFilterStadiumOption)
    };
}

#region Games
var gamesCommand = new Command("games", "Query for games. Returns amount of games played against the total amount of games.");

var playersOption = new Option<string[]>(new[] { "--players", "-p" }, "Look for games where any of these players participated.") { Arity = ArgumentArity.ZeroOrMore, AllowMultipleArgumentsPerToken = true };
var authOption = new Option<bool>(new[] { "--auth", "-a" }, () => false, "Look for public Auth instead of name. See https://www.haxball.com/playerauth");
var teamOption = new Option<bool>(new[] { "--team", "-t" }, () => false, "Look for games where all of the given players were in one team.");
var fromOption = new Option<DateTime>("--from", () => DateTime.MinValue, "Only count games played since the give time.");
var toOption = new Option<DateTime>("--to", () => DateTime.MaxValue, "Only count games played until the given time.");
var undecidedOption = new Option<bool>(new[] { "--undecided", "-u" }, () => false, "Include undecided games.");
var stadiumOption = new Option<string>(new[] { "--stadium", "-s" }, () => string.Empty, "Look for games played on given stadium. Returns non-exact case-sensitive matches.");
var queryFilterBinder = new QueryFilterBinder(playersOption, authOption, teamOption, fromOption, toOption, undecidedOption, stadiumOption);

gamesCommand.AddGlobalOptions(playersOption, authOption, teamOption, fromOption, toOption, undecidedOption, stadiumOption);
gamesCommand.SetHandler((QueryFilter filter, ParseResult parseResult) => 
{
    var preFilter = GetPreFilter(parseResult);
    app.Games(preFilter, filter);
}, queryFilterBinder);

#region Overview
var overviewCommand = new Command("overview", "Prints an overview for each of the given options.");
var byPlayerOption = new Option<bool>(new[] { "--by-player" }, () => true, "Group by player");
var byTeamOption = new Option<bool>(new[] { "--by-team" }, () => false, "Group by team");
var byStadiumOption = new Option<bool>(new[] { "--by-stadium" }, () => false, "Group by stadium");
var byDayOption = new Option<bool>(new[] { "--by-day" }, () => false, "Group by day");
var statCollectorBinder = new StatsPrinterBinder(byPlayerOption, byTeamOption, byStadiumOption, byDayOption);
overviewCommand.AddOptions(byPlayerOption, byTeamOption, byStadiumOption, byDayOption);
overviewCommand.SetHandler((QueryFilter filter, StatsPrinter collector) => app.Overview(filter, collector), queryFilterBinder, statCollectorBinder);
#endregion

#region Won
var wonCommand = new Command("won", "Count the amount of games won against the total amount of filtered games.");
var redWonOption = new Option<bool>(new[] { "--red", "-r" }, () => false, "Constrain to games won by the red team.");
var blueWonOption = new Option<bool>(new[] { "--blue", "-b" }, () => false, "Constrain to games won by the blue team.");
wonCommand.AddOptions(redWonOption, blueWonOption);
wonCommand.SetHandler((QueryFilter filter, bool redWon, bool blueWon, ParseResult parseResult) => app.WonOrLost(GameResult.Won)(filter, redWon, blueWon), queryFilterBinder, redWonOption, blueWonOption);
#endregion

#region Lost
var lostCommand = new Command("lost", "Count the amount of games lost against the total amount of filtered games.");
var redLostOption = new Option<bool>(new[] { "--red", "-r" }, () => false, "Constrain to games lost by the red team.");
var blueLostOption = new Option<bool>(new[] { "--blue", "-b" }, () => false, "Constrain to games lost by the blue team.");
lostCommand.AddOptions(redLostOption, blueLostOption);
lostCommand.SetHandler((QueryFilter filter, bool redLost, bool blueLost, ParseResult parseResult) => app.WonOrLost(GameResult.Lost)(filter, redLost, blueLost), queryFilterBinder, redLostOption, blueLostOption);
#endregion

gamesCommand.Add(overviewCommand);
gamesCommand.Add(wonCommand);
gamesCommand.Add(lostCommand);
#endregion

queryCommand.Add(gamesCommand);
#endregion

rootCommand.Add(queryCommand);
rootCommand.Add(createCommand);

app.Init();
await rootCommand.InvokeAsync(Environment.GetCommandLineArgs());