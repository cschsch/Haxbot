namespace Haxbot.Stats;

public record FlattenedGameStats : GameStats
{
    public DateTime? Date { get; set; }
    public Stadium? Stadium { get; set; }

    public FlattenedGameStats(GameStats stats) : base(stats)
    {
    }
}