using Microsoft.Extensions.Configuration;

namespace Haxbot.Settings;

public static class HaxbotSettings
{
    private static Configuration Configuration { get; set; } = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(Environment.GetCommandLineArgs())
                .Build()
                .Get<Configuration>();

    public static string DatabasePath => Configuration.DatabasePath;
    public static string ConnectionString => string.Format(Configuration.ConnectionStringTemplate, new[] { Configuration.DatabasePath });
    public static string HaxballHeadlessUrl => Configuration.HaxballHeadlessUrl;
    public static RoomConfiguration RoomConfiguration => Configuration.RoomConfiguration;
    public static string[] Admins => Configuration.GameAdmins;
    public static string BotOwner => Configuration.BotOwner;
}
