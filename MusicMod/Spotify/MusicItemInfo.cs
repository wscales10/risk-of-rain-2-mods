using SpotifyAPI.Web;
using System.Collections.Generic;

namespace Spotify
{
	public class MusicItemInfo
	{
		public MusicItemInfo(SpotifyItem musicItem)
		{
			PreviewItem = MusicItem = musicItem;
		}

		public SpotifyItem MusicItem { get; }

		public List<Image> Images { get; set; }

		public string Name { get; set; }

		public (string name, SpotifyItem item)[] Creators { get; set; }

		public SpotifyItem PreviewItem { get; set; }
	}
}
