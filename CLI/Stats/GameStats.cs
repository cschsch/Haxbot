using CLI.Commands;

namespace CLI.Stats;

public record GameStats
{
    public string Identification { get; init; } = string.Empty;
    public int AmountWon { get; init; }
    public int AmountLost { get; init; }
    public int AmountPlayed => AmountWon + AmountLost;
    public decimal Winrate => AmountPlayed == 0 ? -1 : Math.Round((decimal)AmountWon / AmountPlayed, 2);
    public decimal WinLoseRatio => AmountLost == 0 ? -1 : Math.Round((decimal)AmountWon / AmountLost, 2);

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
