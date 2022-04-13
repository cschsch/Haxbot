namespace Haxbot.Stats;

public record FlattenedGameStats : GameStats
{
    public DateTime? Date { get; set; }
    public Stadium? Stadium { get; set; }

    public FlattenedGameStats() { }

    public FlattenedGameStats(GameStats stats) : base(stats)
    {
    }

    public FlattenedGameStats Add(FlattenedGameStats stats) => this with
    {
        AmountWon = AmountWon + stats.AmountWon,
        AmountLost = AmountLost + stats.AmountLost,
        Date = stats.Date,
        Stadium = stats.Stadium,
        Identification = stats.Identification
    };
}