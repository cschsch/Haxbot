namespace Haxbot.Api;

public class HaxballApiFunctions
{
    public HaxbotContext Context { get; }

    public HaxballApiFunctions(HaxbotContext context)
    {
        Context = context;
    }

    public bool AddPlayer(HaxballPlayer player)
    {
        return true;
    }

    public bool AddFinishedGame(HaxballScores scores)
    {
        return true;
    }
}