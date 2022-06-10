using System.Collections.Generic;

namespace Spotify.Commands.Interfaces
{
    public interface ICommandList : IEnumerable<ICommand>
    {
    }
}