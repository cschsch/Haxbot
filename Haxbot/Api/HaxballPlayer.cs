namespace Haxbot.Api;

public class HaxballPlayer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public TeamId Team { get; set; }
    public string Auth { get; set; }

    public HaxballPlayer()
    {
        Name = string.Empty;
        Auth = string.Empty;
    }
}