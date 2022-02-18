using System.CommandLine;
using System.CommandLine.Invocation;

namespace CLI;

public static class CommandExtensions
{
    public static Command WithHandler(this Command command, ICommandHandler? handler)
    {
        command.Handler = handler;
        return command;
    }
}