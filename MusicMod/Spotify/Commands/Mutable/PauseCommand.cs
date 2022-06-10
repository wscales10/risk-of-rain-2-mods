using Spotify.Commands.Interfaces;

namespace Spotify.Commands.Mutable
{
    public class PauseCommand : Command, IReadOnlyCommand
    {
        public override IReadOnlyCommand ToReadOnly() => this;
    }
}