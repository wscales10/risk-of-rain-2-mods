using System.Threading.Tasks;
using Utils.Async;

namespace Spotify.Commands
{
	public class CommandListAsync : BlockableExecutable
	{
		public CommandListAsync(ICommandList commands, Task blocker) : base(blocker)
		{
			Commands = commands;
		}

		public ICommandList Commands { get; }
	}
}