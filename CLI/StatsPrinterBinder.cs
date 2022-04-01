using System.CommandLine;
using System.CommandLine.Binding;

namespace CLI;

public class StatsPrinterBinder : BinderBase<StatsPrinter>
{
    private readonly Option<bool> _byPlayer;
    private readonly Option<bool> _byTeam;
    private readonly Option<bool> _byStadium;
    private readonly Option<bool> _byDay;

    public StatsPrinterBinder(Option<bool> byPlayer, Option<bool> byTeam, Option<bool> byStadium, Option<bool> byDay)
    {
        _byPlayer = byPlayer;
        _byTeam = byTeam;
        _byStadium = byStadium;
        _byDay = byDay;
    }

    protected override StatsPrinter GetBoundValue(BindingContext bindingContext)
    {
        return new StatsPrinter
        {
            ByPlayer = bindingContext.ParseResult.GetValueForOption(_byPlayer),
            ByTeam = bindingContext.ParseResult.GetValueForOption(_byTeam),
            ByStadium = bindingContext.ParseResult.GetValueForOption(_byStadium),
            ByDay = bindingContext.ParseResult.GetValueForOption(_byDay)
        };
    }
}
