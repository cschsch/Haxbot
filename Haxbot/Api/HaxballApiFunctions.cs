using Haxbot.Entities;

namespace Haxbot.Api;

public class HaxballApiFunctions
{
    public HaxbotContext Context { get; }

    public HaxballApiFunctions(HaxbotContext context)
    {
        Context = context;
    }

    public bool StartGame(HaxballPlayer[] players)
    {
        return true;
    }

    public bool FinishGame(HaxballScores scores)
    {
        return true;
    }

    public string HandleCommand(HaxballPlayer player, string message)
    {
        return message;
    }
}