namespace Haxbot.Api;

public record HaxballPlayer
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Auth { get; init; } = string.Empty;
    public TeamId Team { get; init; }

    public HaxballPlayer EnrichAuth(string[][] idAuths) => this with
    {
        Auth = idAuths.Single(idAuth => int.Parse(idAuth[0]) == Id)[1]
    };

    public virtual bool Equals(HaxballPlayer? other) => 
        other != null && 
        Id == other.Id && 
        Name == other.Name && 
        Auth == other.Auth;

    public override int GetHashCode() => 
        unchecked(Id.GetHashCode() * 17 + Name.GetHashCode() * 17 + Auth.GetHashCode());
}