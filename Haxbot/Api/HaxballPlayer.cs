namespace Haxbot.Api;

public record HaxballPlayer
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public TeamId Team { get; init; }
    public string Auth { get; init; } = default!;

    public HaxballPlayer EnrichAuth(string[][] idAuths) => this with 
    { 
        Auth = idAuths.Single(idAuth => int.Parse(idAuth[0]) == Id)[1] 
    };
}