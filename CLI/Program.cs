using CLI;
using Haxbot.Settings;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;

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
    { nameof(Configuration.RoomAdmins), new [] { "--room-admins", "-a" } },
    { nameof(Configuration.BotOwner), new [] { "--bot-owner", "-o" } }
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
var app = new HaxbotApp(configuration);

var rootCommand = new RootCommand("Create a haxball room and output its url or query for statistics gathered in such rooms.")
{
    new Option<string>(mainSwitches[nameof(Configuration.DatabasePath)], "Path pointing to database to be used. May not exist yet."),
    new Option<string>(mainSwitches[nameof(Configuration.ConnectionStringTemplate)], "string.Format to be used to connect to the database. Can be used to inject arguments. DatabasePath will be inserted at {0}."),

    new Command("create", "Create a new room and output its url.")
    {
        new Option<string?>(new[] { "--token", "-t" }, "Haxball API token. Required if headless. Obtain here: https://www.haxball.com/headlesstoken"),
        new Option<bool>("--headless", "Whether to run the browser headless or not."),
        new Option<string>(createRoomSwitches[nameof(Configuration.HaxballHeadlessUrl)], "URL pointing to Haxball headless API"),
        new Option<string>(createRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.RoomName)}"], "Name of room to be created"),
        new Option<string>(createRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Password)}"], "Password to be assigned to room"),
        new Option<bool>(createRoomSwitches[$"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Public)}"], "Whether to make the room public"),
        new Option<string[]>(createRoomSwitches[nameof(Configuration.RoomAdmins)], "Comma seperated list of auths of players to be made room admins"),
        new Option<string>(createRoomSwitches[nameof(Configuration.BotOwner)], "Auth of owner of bot")
    }.WithHandler(CommandHandler.Create<string?, bool>(app.CreateRoom))
};

app.Init();
await rootCommand.InvokeAsync(Environment.GetCommandLineArgs());