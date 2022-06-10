using System;

namespace Spotify.Commands.Interfaces
{
    public interface IPlayCommand : ICommand
    {
        TimeSpan? At { get; }

        ISpotifyItem Item { get; }

        int Milliseconds { get; }

        IPlayCommand AtMilliseconds(int ms);

        IPlayCommand AtSeconds(int s);
    }
}