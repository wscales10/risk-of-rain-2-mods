using Spotify.Commands.Interfaces;
using Spotify.Commands.Mutable;
using System;
using System.Threading.Tasks;

namespace Spotify.Commands.ReadOnly
{
    public class ReadOnlyGetMusicItemInfoCommand : ReadOnlyCommand, IGetMusicItemInfoCommand
    {
        public ReadOnlyGetMusicItemInfoCommand(GetMusicItemInfoCommand mutable) : base(mutable)
        {
            Callback = mutable.Callback;
            Item = mutable.Item;
            Task = mutable.Task;
        }

        public Func<MusicItemInfo, Task> Callback { get; }

        public SpotifyItem Item { get; }

        public Task Task { get; internal set; }
    }
}