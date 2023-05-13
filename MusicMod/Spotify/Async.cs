using Utils.Async;

namespace Spotify
{
	internal static class Async
	{
		internal static AsyncManager Manager { get; } = new AsyncManager();
	}
}