using CLI;
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
    { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Public)}", new [] { "--public" } }
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
};
var tokenOption = new Option<string?>(new[] { "--token", "-t" }, "Haxball API token. Required if headless. Obtain here: https://www.haxball.com/headlesstoken");
var headlessOption = new Option<bool>("--headless", () => false, "Whether to run the browser headless or not.");
createCommand.AddOptions(tokenOption, headlessOption);
createCommand.SetHandler((string? token, bool headless) => app.CreateRoom(token, headless), tokenOption, headlessOption);
#endregion

#region Query
var queryCommand = new Command("query", "Query the database");

#region Names
var namesCommand = new Command("names", "Given a public Auth, get all associated names.");
var authArgument = new Argument<string>("auth", "Public Auth value of player. See https://www.haxball.com/playerauth");
namesCommand.Add(authArgument);
namesCommand.SetHandler((string auth) => app.Names(auth), authArgument);
#endregion

#region Games
var gamesCommand = new Command("games", "Query for games. Returns amount of games played.");
var playersOption = new Option<string[]>(new[] { "--players", "-p" }, "Look for games where any of these players participated.") { Arity = ArgumentArity.ZeroOrMore, AllowMultipleArgumentsPerToken = true };
var authOption = new Option<bool>(new[] { "--auth", "-a" }, () => false, "Look for public Auth instead of name. See https://www.haxball.com/playerauth");
var teamOption = new Option<bool>(new[] { "--team", "-t" }, () => false, "Look for games where all of the given players were in one team.");
var fromOption = new Option<DateTime>("--from", () => DateTime.MinValue, "Only count games played since the give time.");
var toOption = new Option<DateTime>("--to", () => DateTime.MaxValue, "Only count games played until the given time.");
gamesCommand.AddGlobalOptions(playersOption, authOption, teamOption, fromOption, toOption);
gamesCommand.SetHandler((string[] players, bool auth, bool team, DateTime from, DateTime to) => app.Games(players, auth, team, from, to), playersOption, authOption, teamOption, fromOption, toOption);

#region Won
var wonCommand = new Command("won", "Count the amount of games won.");
var redWonOption = new Option<bool>(new[] { "--red", "-r" }, () => false, "Constrain to games won by the red team.");
var blueWonOption = new Option<bool>(new[] { "--blue", "-b" }, () => false, "Constrain to games won by the blue team.");
wonCommand.AddOptions(redWonOption, blueWonOption);
wonCommand.SetHandler((string[] players, bool auth, bool team, DateTime from, DateTime to, bool redWon, bool blueWon) => app.WonOrLost(GameResult.Won)(players, auth, team, from, to, redWon, blueWon), playersOption, authOption, teamOption, fromOption, toOption, redWonOption, blueWonOption);
#endregion

#region Lost
var lostCommand = new Command("lost", "Count the amount of games lost.");
var redLostOption = new Option<bool>(new[] { "--red", "-r" }, () => false, "Constrain to games lost by the red team.");
var blueLostOption = new Option<bool>(new[] { "--blue", "-b" }, () => false, "Constrain to games lost by the blue team.");
lostCommand.AddOptions(redLostOption, blueLostOption);
lostCommand.SetHandler((string[] players, bool auth, bool team, DateTime from, DateTime to, bool redLost, bool blueLost) => app.WonOrLost(GameResult.Lost)(players, auth, team, from, to, redLost, blueLost), playersOption, authOption, teamOption, fromOption, toOption, redLostOption, blueLostOption);
#endregion

gamesCommand.Add(wonCommand);
gamesCommand.Add(lostCommand);
#endregion

queryCommand.Add(namesCommand);
queryCommand.Add(gamesCommand);
#endregion

rootCommand.Add(queryCommand);
rootCommand.Add(createCommand);

app.Init();
await rootCommand.InvokeAsync(Environment.GetCommandLineArgs());