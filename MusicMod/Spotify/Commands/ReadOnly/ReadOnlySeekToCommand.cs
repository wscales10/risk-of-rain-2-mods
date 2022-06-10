using Spotify.Commands.Interfaces;
using Spotify.Commands.Mutable;
using System;

namespace Spotify.Commands.ReadOnly
{
    public class ReadOnlySeekToCommand : ReadOnlyCommand, ISeekToCommand
    {
        public ReadOnlySeekToCommand(SeekToCommand mutable) : base(mutable)
        {
            Milliseconds = mutable.Milliseconds;
            At = mutable.At;
        }

        public int Milliseconds { get; }

        public TimeSpan At { get; set; }
    }
}