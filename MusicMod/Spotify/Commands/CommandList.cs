using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace Spotify.Commands
{
	public class CommandList : List<Command>, ICommandList
	{
		public CommandList() : this(Enumerable.Empty<Command>())
		{
		}

		public CommandList(params Command[] commands) : base(commands)
		{
		}

		public CommandList(IEnumerable<Command> commands) : base(commands)
		{
		}

		public static implicit operator CommandList(Command command) => new CommandList(command);

		public static implicit operator CommandList(Command[] commands) => new CommandList(commands);

		public static CommandList Parse(XElement xml)
		{
			if (xml.Name != "Commands")
			{
				throw new ArgumentOutOfRangeException(nameof(xml), xml, "Element name should be 'Commands'");
			}

			return new CommandList(xml.Elements().Select(Command.FromXml));
		}

		public ReadOnlyCommandList ToReadOnly() => new ReadOnlyCommandList(this);

		public XElement ToXml() => ToXml(this);

		public override string ToString() => string.Join(", ", this);

		internal static XElement ToXml(IEnumerable<Command> commands) => new XElement("Commands", commands.Select(c => c.ToXml()).Cast<object>().ToArray());
	}

	public class ReadOnlyCommandList : ReadOnlyCollection<Command>, ICommandList
	{
		public ReadOnlyCommandList(ICommandList commands) : base(commands.ToList())
		{
		}

		public XElement ToXml() => CommandList.ToXml(this);

		public override string ToString() => string.Join(", ", this);
	}
}