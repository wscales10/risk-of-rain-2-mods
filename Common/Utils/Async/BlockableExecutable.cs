using System.Threading.Tasks;

namespace Utils.Async
{
	public class BlockableExecutable
	{
		public BlockableExecutable(Task blocker) => Blocker = blocker;

		public Task Blocker { get; }
	}
}