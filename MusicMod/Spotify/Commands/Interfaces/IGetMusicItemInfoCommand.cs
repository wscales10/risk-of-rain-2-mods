using System;
using System.Threading.Tasks;

namespace Spotify.Commands.Interfaces
{
    public interface IGetMusicItemInfoCommand : ICommand
    {
        Func<MusicItemInfo, Task> Callback { get; }

        SpotifyItem Item { get; }

        Task Task { get; }
    }
}