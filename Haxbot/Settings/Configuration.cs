using System.Text.Json.Serialization;

namespace Haxbot.Settings;

public record Configuration
{
    public string DatabasePath { get; init; }
    public string ConnectionStringTemplate { get; init; }
    public string HaxballHeadlessUrl { get; init; }
    public string[] ChromiumArgs { get; init; }
    public RoomConfiguration RoomConfiguration { get; init; }
    public string[] RoomAdmins { get; init; }
    public string BotOwner { get; init; }
    public string[] StandardOverviewCommands { get; init; }

    [JsonIgnore]
    public string ConnectionString => string.Format(ConnectionStringTemplate, new[] { DatabasePath });

    public Configuration()
    {
        DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(Haxbot), "haxbot.db");
        ConnectionStringTemplate = "Data Source={0}";
        HaxballHeadlessUrl = "https://html5.haxball.com/headless";
        ChromiumArgs = new[] { "--disable-features=WebRtcHideLocalIpsWithMdns", "--no-sandbox", "--no-proxy-server" };
        RoomConfiguration = new RoomConfiguration();
        RoomAdmins = Array.Empty<string>();
        BotOwner = string.Empty;
        StandardOverviewCommands = new[] 
        {
            "query games --to {0} overview player",
            "query games overview player",
            "query games overview --to {0} team",
            "query games overview team",
            "query games overview stadium player",
            "query games overview stadium team"
        };
    }
}