using CLI.Extensions;
using System.CommandLine;
using System.CommandLine.Binding;

namespace CLI;

public class QueryCommand : BinderBase<QueryFilter>
{
    public Option<string[]> Players { get; init; } = new Option<string[]>(new[] { "--players", "-p" }, "Look for games where any of these players participated.") { Arity = ArgumentArity.ZeroOrMore, AllowMultipleArgumentsPerToken = true };
    public Option<bool> Auth { get; init; } = new Option<bool>(new[] { "--auth", "-a" }, () => false, "Look for public Auth instead of name. See https://www.haxball.com/playerauth");
    public Option<bool> Team { get; init; } = new Option<bool>(new[] { "--team", "-t" }, () => false, "Look for games where all of the given players were in one team.");
    public Option<DateTime> From { get; init; } = new Option<DateTime>("--from", () => DateTime.MinValue, "Only count games played since the give time.");
    public Option<DateTime> To { get; init; } = new Option<DateTime>("--to", () => DateTime.MaxValue, "Only count games played until the given time.");
    public Option<bool> Undecided { get; init; } = new Option<bool>(new[] { "--undecided", "-u" }, () => false, "Include undecided games.");
    public Option<string> Stadium { get; init; } = new Option<string>(new[] { "--stadium", "-s" }, () => string.Empty, "Look for games played on given stadium. Returns non-exact case-sensitive matches.");

    public Command GetCommand(string name, string description, bool global = false)
    {
        var command = new Command(name, description);
        if (global)
        {
            command.AddGlobalOptions(Players, Auth, Team, From, To, Undecided, Stadium);
        }
        else
        {
            command.AddOptions(Players, Auth, Team, From, To, Undecided, Stadium);
        }
        return command;
    }

    protected override QueryFilter GetBoundValue(BindingContext bindingContext)
    {
        return new QueryFilter
        {
            Players = bindingContext.ParseResult.GetValueForOption(Players),
            Auth = bindingContext.ParseResult.GetValueForOption(Auth),
            Team = bindingContext.ParseResult.GetValueForOption(Team),
            From = bindingContext.ParseResult.GetValueForOption(From),
            To = bindingContext.ParseResult.GetValueForOption(To),
            Undecided = bindingContext.ParseResult.GetValueForOption(Undecided),
            Stadium = bindingContext.ParseResult.GetValueForOption(Stadium)
        };
    }
}