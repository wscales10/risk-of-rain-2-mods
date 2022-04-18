using System;
using System.Threading.Tasks;

namespace Spotify.Commands
{
	public class GetMusicItemInfoCommand : Command
	{
		public GetMusicItemInfoCommand(SpotifyItem item, Func<MusicItemInfo, Task> callback)
		{
			Item = item;
			Callback = callback;
		}

		public SpotifyItem Item { get; }

		public Func<MusicItemInfo, Task> Callback { get; }

		public Task Task { get; internal set; }
	}
}
