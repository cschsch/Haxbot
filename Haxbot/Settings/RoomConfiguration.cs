namespace Haxbot.Settings;

public record RoomConfiguration
{
    public string RoomName { get; init; } = string.Empty;
    public string? Password { get; init; } = default;
    public bool Public { get; init; }
    public bool NoPlayer { get; init; } = true;
}