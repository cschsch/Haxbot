namespace Haxbot.Api;

public record TeamSetup(HaxballPlayer[] Red, HaxballPlayer[] Blue)
{
    public int TotalCount => Red.Length + Blue.Length;
    public static TeamSetup Default { get; } = new(Array.Empty<HaxballPlayer>(), Array.Empty<HaxballPlayer>());
}
