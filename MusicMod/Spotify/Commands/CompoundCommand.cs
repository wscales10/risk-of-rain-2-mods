using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public class CompoundCommand : Command
	{
		public CompoundCommand(XElement element)
		{
			Commands = element.Elements().Select(FromXml).ToReadOnlyCollection();
		}

		public CompoundCommand(params Command[] commands)
		{
			Commands = commands.ToReadOnlyCollection();
		}

		public CompoundCommand(IEnumerable<Command> commands)
		{
			Commands = commands.ToReadOnlyCollection();
		}

		public ReadOnlyCollection<Command> Commands { get; }

		public IEnumerable<Command> Unwrap()
		{
			foreach (var command1 in Commands)
			{
				if(command1 is CompoundCommand compoundCommand)
				{
					foreach (var command2 in compoundCommand.Unwrap())
					{
						yield return command2;
					}
				}
				else
				{
					yield return command1;
				}
			}
		}

		internal static Command Parse(XElement element)
		{
			throw new NotImplementedException();
		}

		protected override void AddDetail(XElement element)
		{
			element.Add(Commands.Select(c => c.ToXml()).ToArray());
		}
	}
}
