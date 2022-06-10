using Spotify.Commands.Interfaces;
using Spotify.Commands.Mutable;
using System;

namespace Spotify.Commands.ReadOnly
{
    public class ReadOnlyPlayCommand : ReadOnlyCommand, IPlayCommand
    {
        public ReadOnlyPlayCommand(LoopCommand mutable) : base(mutable)
        {
        }

        public TimeSpan? At { get; }

        public ISpotifyItem Item { get; }

        public int Milliseconds { get; }

        public IPlayCommand AtMilliseconds(int ms) => new LoopCommand(Item).AtMilliseconds(ms).ToReadOnly();

        public IPlayCommand AtSeconds(int s)
        {
            throw new NotImplementedException();
        }
    }
}