using System.Text.Json.Serialization;

namespace Haxbot.Settings;

public record Configuration
{
    public string DatabasePath { get; init; }
    public string ConnectionStringTemplate { get; init; }
    public string HaxballHeadlessUrl { get; init; }
    public RoomConfiguration RoomConfiguration { get; init; }
    public string[] GameAdmins { get; init; }
    public string BotOwner { get; init; }

    [JsonIgnore]
    public string ConnectionString => string.Format(ConnectionStringTemplate, new[] { DatabasePath });

    public Configuration()
    {
        DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(Haxbot), "haxbot.db");
        ConnectionStringTemplate = "Data Source={0}";
        HaxballHeadlessUrl = "https://html5.haxball.com/headless";
        RoomConfiguration = new RoomConfiguration();
        GameAdmins = Array.Empty<string>();
        BotOwner = string.Empty;
    }
}