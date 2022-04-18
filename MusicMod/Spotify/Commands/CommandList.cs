using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Spotify.Commands
{
	public class CommandList : List<Command>, ICommandList
	{
		public CommandList(params Command[] commands) : base(commands) { }

		public CommandList(IEnumerable<Command> commands) : base(commands) { }

		public static implicit operator CommandList(Command command) => new CommandList(command);

		public static implicit operator CommandList(Command[] commands) => new CommandList(commands);

		public ReadOnlyCommandList ToReadOnly() => new ReadOnlyCommandList(this);
	}

	public class ReadOnlyCommandList : ReadOnlyCollection<Command>, ICommandList
	{
		public ReadOnlyCommandList(ICommandList commands) : base(commands.ToList()) { }
	}
}
