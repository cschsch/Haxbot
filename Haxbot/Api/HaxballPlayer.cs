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

    public HaxballPlayer EnrichAuth(string[][] idAuths)
    {
        Auth = idAuths.Single(idAuth => int.Parse(idAuth[0]) == Id)[1];
        return this;
    }
}