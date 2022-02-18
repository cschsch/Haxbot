using CLI;
using Haxbot.Settings;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(Environment.GetCommandLineArgs())
    .Build()
    .Get<Configuration>();
var app = new HaxbotApp(configuration);

var rootCommand = new RootCommand("Create a haxball room and output its url or query for statistics gathered in such rooms.")
{
    new Command("create", "Create a new room and output its url.")
    {
        new Option<string?>(new[] { "--token", "-t" }, "Haxball API token. Required if headless. Obtain here: https://www.haxball.com/headlesstoken"),
        new Option<bool>("--headless", "Whether to run the browser headless or not. Defaults to false if no token was provided.")
    }.WithHandler(CommandHandler.Create<string?, bool>(app.CreateRoom))
};

app.Init();
await rootCommand.InvokeAsync(Environment.GetCommandLineArgs());