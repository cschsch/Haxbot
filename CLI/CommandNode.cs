using System.Collections;
using System.CommandLine;

namespace CLI;

public class CommandNode : ICollection<CommandNode>
{
    public Command Command { get; init; }
    private List<CommandNode> SubCommands { get; } = new List<CommandNode>();

    public CommandNode(Command command) => Command = command;

    public int Count => 1 + SubCommands.Sum(commandNode => commandNode.Count);
    public bool IsReadOnly => false;

    public void Add(CommandNode item) => SubCommands.Add(item);

    public void Clear() => SubCommands.Clear();

    public bool Contains(CommandNode item) => SubCommands.Contains(item);

    public void CopyTo(CommandNode[] array, int arrayIndex) => SubCommands.CopyTo(array, arrayIndex);

    public IEnumerator<CommandNode> GetEnumerator() => SubCommands.GetEnumerator();

    public bool Remove(CommandNode item) => SubCommands.Remove(item);

    IEnumerator IEnumerable.GetEnumerator() => SubCommands.GetEnumerator();

    public static implicit operator CommandNode(Command command) => new(command);

    public void InitializeCommand()
    {
        foreach (var command in SubCommands)
        {
            Command.AddCommand(command.Command);
            command.InitializeCommand();
        }
    }
}
