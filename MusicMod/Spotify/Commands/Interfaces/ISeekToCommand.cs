using System;

namespace Spotify.Commands.Interfaces
{
    public interface ISeekToCommand : ICommand
    {
        int Milliseconds { get; }

        TimeSpan At { get; }
    }
}