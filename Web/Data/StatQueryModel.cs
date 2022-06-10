using Haxbot.Stats;

namespace Web.Data;

public class StatQueryModel : GamesQueryModel
{
    public IReadOnlyList<Grouping> PossibleGroupings { get; init; } = new List<Grouping>();
    public Grouping ChosenResultGrouping { get; set; } = Grouping.Player;
}