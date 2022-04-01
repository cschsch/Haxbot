using System.CommandLine;

namespace CLI.Extensions;

public static class CommandExtensions
{
    public static Command AddOptions(this Command command, params Option[] options)
    {
        foreach (var option in options)
        {
            command.AddOption(option);
        }
        return command;
    }

    public static Command AddGlobalOptions(this Command command, params Option[] options)
    {
        foreach (var option in options)
        {
            command.AddGlobalOption(option);
        }
        return command;
    }
}
