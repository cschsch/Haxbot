namespace Haxbot.Stats;

public record GameStats
{
    public string Identification { get; init; } = string.Empty;
    public int AmountWon { get; init; }
    public int AmountLost { get; init; }
    public int AmountPlayed => AmountWon + AmountLost;
    public decimal Winrate => AmountPlayed == 0 ? -1 : (decimal)AmountWon / AmountPlayed * 100;
    public decimal WinLoseRatio => AmountLost == 0 ? -1 : (decimal)AmountWon / AmountLost;

    public GameStats RegisterGame(GameResult result)
    {
        return result switch
        {
            GameResult.Won => this with { AmountWon = AmountWon + 1 },
            GameResult.Lost => this with { AmountLost = AmountLost + 1 },
            _ => this,
        };
    }
}
