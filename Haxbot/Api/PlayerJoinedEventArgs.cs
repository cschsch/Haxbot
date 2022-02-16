namespace Haxbot.Api;

public class PlayerJoinedEventArgs : EventArgs
{
    public HaxballPlayer Player { get; }
    
    public PlayerJoinedEventArgs(HaxballPlayer player) => Player = player;
}
