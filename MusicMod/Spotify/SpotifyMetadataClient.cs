using Spotify.Commands;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Spotify
{
    public class SpotifyMetadataClient : NiceSpotifyClient
    {
        public SpotifyMetadataClient(Logger logger) : base(logger)
        {
        }

        protected async override Task<bool> HandleAsync(Command command, CancellationToken? cancellationToken)
        {
            switch (command)
            {
                case GetMusicItemInfoCommand c:
                    MusicItemInfo info = null;

                    try
                    {
                        info = await GetMusicItemInfoAsync(c.Item);
                    }
                    finally
                    {
                        c.Task = c.Callback(info);
                    }

                    return true;
            }

            return false;
        }

        private async Task<MusicItemInfo> GetMusicItemInfoAsync(SpotifyItem item)
        {
            switch (item.Type)
            {
                case SpotifyItemType.Track:
                    var fullTrack = await Client.Tracks.Get(item.Id);
                    return new MusicItemInfo(item)
                    {
                        Name = fullTrack.Name,
                        Images = fullTrack.Album.Images,
                        Creators = fullTrack.Artists.Select(a => new Creator(a.Name, new SpotifyItem(SpotifyItemType.Artist, a.Id))).ToArray(),
                        PreviewItem = new SpotifyItem(SpotifyItemType.Album, fullTrack.Album.Id)
                    };

                case SpotifyItemType.Playlist:
                    var playlist = await Client.Playlists.Get(item.Id);
                    return new MusicItemInfo(item)
                    {
                        Name = playlist.Name,
                        Images = playlist.Images,
                        Creators = new[] { new Creator(playlist.Owner.DisplayName, new SpotifyItem(SpotifyItemType.User, playlist.Owner.Id)) },
                    };

                case SpotifyItemType.Album:
                    var album = await Client.Albums.Get(item.Id);
                    return new MusicItemInfo(item)
                    {
                        Name = album.Name,
                        Images = album.Images,
                        Creators = album.Artists.Select(a => new Creator(a.Name, new SpotifyItem(SpotifyItemType.Artist, a.Id))).ToArray(),
                    };

                case SpotifyItemType.Artist:
                    var artist = await Client.Artists.Get(item.Id);
                    return new MusicItemInfo(item)
                    {
                        Name = artist.Name,
                        Images = artist.Images,
                    };

                default:
                    return null;
            }
        }
    }
}