using SpotifyAPI.Web;
using System.Collections.Generic;

namespace Spotify
{
    public struct Creator
    {
        public Creator(string name, SpotifyItem spotifyItem) : this()
        {
            Name = name;
            SpotifyItem = spotifyItem;
        }

        public string Name { get; }

        public SpotifyItem SpotifyItem { get; }
    }

    public class MusicItemInfo
    {
        public MusicItemInfo(SpotifyItem musicItem)
        {
            PreviewItem = MusicItem = musicItem;
        }

        public SpotifyItem MusicItem { get; }

        public List<Image> Images { get; set; }

        public string Name { get; set; }

        public Creator[] Creators { get; set; }

        public SpotifyItem PreviewItem { get; set; }
    }
}