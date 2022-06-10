using Spotify.Commands.Interfaces;
using System;
using System.Threading.Tasks;

namespace Spotify.Commands.Mutable
{
    public class GetMusicItemInfoCommand : Command, IGetMusicItemInfoCommand
    {
        public GetMusicItemInfoCommand(SpotifyItem item, Func<MusicItemInfo, Task> callback)
        {
            Item = item;
            Callback = callback;
        }

        public SpotifyItem Item { get; }

        public Func<MusicItemInfo, Task> Callback { get; }

        public Task Task { get; internal set; }

        public override IReadOnlyCommand ToReadOnly() => new ReadOnlyGetMusicItemInfoCommand(this);
    }
}