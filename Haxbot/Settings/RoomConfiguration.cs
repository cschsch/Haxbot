namespace Haxbot.Settings;

public class RoomConfiguration
{
    public string RoomName { get; set; }
    public string? Password { get; set; }
    public bool Public { get; set; }
    public bool NoPlayer { get; set; }

    public RoomConfiguration()
    {
        RoomName = string.Empty;
        NoPlayer = true;
    }
}