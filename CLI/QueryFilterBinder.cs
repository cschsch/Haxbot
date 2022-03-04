using System.CommandLine;
using System.CommandLine.Binding;

namespace CLI;

public class QueryFilterBinder : BinderBase<QueryFilter>
{
    private readonly Option<string[]> _playersOption;
    private readonly Option<bool> _authOption;
    private readonly Option<bool> _teamOption;
    private readonly Option<DateTime> _fromOption;
    private readonly Option<DateTime> _toOption;
    private readonly Option<bool> _undecidedOption;
    private readonly Option<string> _stadiumOption;

    public QueryFilterBinder(Option<string[]> playersOption, Option<bool> authOption, Option<bool> teamOption, Option<DateTime> fromOption, Option<DateTime> toOption, Option<bool> undecidedOption, Option<string> stadiumOption)
    {
        _playersOption = playersOption;
        _authOption = authOption;
        _teamOption = teamOption;
        _fromOption = fromOption;
        _toOption = toOption;
        _undecidedOption = undecidedOption;
        _stadiumOption = stadiumOption;
    }

    protected override QueryFilter GetBoundValue(BindingContext bindingContext)
    {
        return new QueryFilter
        {
            Players = bindingContext.ParseResult.GetValueForOption(_playersOption),
            Auth = bindingContext.ParseResult.GetValueForOption(_authOption),
            Team = bindingContext.ParseResult.GetValueForOption(_teamOption),
            From = bindingContext.ParseResult.GetValueForOption(_fromOption),
            To = bindingContext.ParseResult.GetValueForOption(_toOption),
            Undecided = bindingContext.ParseResult.GetValueForOption(_undecidedOption),
            Stadium = bindingContext.ParseResult.GetValueForOption(_stadiumOption)
        };
    }
}