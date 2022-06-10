using Haxbot.Stats;
using System.CommandLine;
using System.CommandLine.Binding;

namespace CLI;

public class StatsPrinterBinder : BinderBase<StatsPrinter>
{
    private readonly Argument<Grouping[]> _groupings;

    public StatsPrinterBinder(Argument<Grouping[]> groupings)
    {
        _groupings = groupings;
    }

    protected override StatsPrinter GetBoundValue(BindingContext bindingContext)
    {
        return new StatsPrinter
        {
            Groupings = bindingContext.ParseResult.GetValueForArgument(_groupings)
        };
    }
}
