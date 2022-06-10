using Spotify.Commands.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Spotify.Commands.Mutable
{
    public class CommandList : List<Command>, ICommandList
    {
        public CommandList(params Command[] commands) : base(commands)
        {
        }

        public CommandList(IEnumerable<Command> commands) : base(commands)
        {
        }

        public static implicit operator CommandList(Command command) => new CommandList(command);

        public static implicit operator CommandList(Command[] commands) => new CommandList(commands);

        public ReadOnlyCommandList ToReadOnly() => new ReadOnlyCommandList(this);

        IEnumerator<ICommand> IEnumerable<ICommand>.GetEnumerator() => GetEnumerator();
    }

    public class ReadOnlyCommandList : ReadOnlyCollection<IReadOnlyCommand>, ICommandList
    {
        public ReadOnlyCommandList(ICommandList commands) : base(commands.Select(c => c.ToReadOnly()).ToList())
        {
        }

        IEnumerator<ICommand> IEnumerable<ICommand>.GetEnumerator() => GetEnumerator();
    }
}