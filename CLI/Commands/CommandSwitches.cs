using Haxbot.Settings;

namespace CLI.Commands;

public static class CommandSwitches
{
    /// <summary>
    /// Created for the purpose of Configuration assembly. This way the given options are directly represented inside the configuration.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> RootSwitches { get; } = new Dictionary<string, string[]>
    {
        { nameof(Configuration.DatabasePath), new [] { "--database", "-d" } },
        { nameof(Configuration.ConnectionStringTemplate), new [] { "--connection-string-template", "-c" } }
    };

    /// <summary>
    /// Created for the purpose of Configuration assembly. This way the given options are directly represented inside the configuration.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> CreateRoomSwitches { get; } = new Dictionary<string, string[]>
    {
        { nameof(Configuration.HaxballHeadlessUrl), new [] { "--url", "-u" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.RoomName)}", new [] { "--room-name", "-r" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Password)}", new [] { "--password", "-p" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.Public)}", new [] { "--public" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.NoPlayer)}", new [] { "--no-player" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.TimeLimit)}", new [] { "--time-limit" } },
        { nameof(Configuration.Headless), new [] { "--headless", "-h" } },
        { $"{nameof(Configuration.RoomConfiguration)}:{nameof(RoomConfiguration.PartyManagement)}", new [] { "--party-management", "-m" } }
    };
}
