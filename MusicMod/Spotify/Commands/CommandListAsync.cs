using System.Threading.Tasks;

namespace Spotify.Commands
{
	public class CommandListAsync
	{
		public CommandListAsync(ICommandList commands, Task blocker)
		{
			Commands = commands;
			Blocker = blocker;
		}

		public ICommandList Commands { get; }

		public Task Blocker { get; }
	}
}
