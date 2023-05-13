using Utils.Async;

namespace SpotifyControlWinForms
{
	internal static class Async
	{
		internal static AsyncManager Manager { get; } = new();
	}
}