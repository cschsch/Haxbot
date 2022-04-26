using Haxbot.Settings;

namespace Web.Data;

public record Room(string Url, string[] Admins, RoomConfiguration RoomConfiguration);