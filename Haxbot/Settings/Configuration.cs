namespace Haxbot.Settings;

public class Configuration
{
    public string DatabasePath { get; set; }
    public string ConnectionStringTemplate { get; set; }
    public string HaxballHeadlessUrl { get; set; }
    public RoomConfiguration RoomConfiguration { get; set; }
    public string[] GameAdmins { get; set; }
    public string BotOwner { get; set; }

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