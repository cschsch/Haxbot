using System.Collections;
using System.CommandLine;

namespace CLI;

public class CommandNode : IEnumerable<CommandNode>
{
    public Command Command { get; init; }
    private List<CommandNode> SubCommands { get; } = new List<CommandNode>();

    public CommandNode(Command command) => Command = command;

    public void Add(CommandNode item) => SubCommands.Add(item);

    public static implicit operator CommandNode(Command command) => new(command);

    public void InitializeCommand()
    {
        foreach (var command in SubCommands)
        {
            Command.AddCommand(command.Command);
            command.InitializeCommand();
        }
    }

    public IEnumerator<CommandNode> GetEnumerator() => SubCommands.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => SubCommands.GetEnumerator();
}
