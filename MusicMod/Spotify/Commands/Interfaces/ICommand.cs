using System.Collections.Generic;
using Utils;

namespace Spotify.Commands.Interfaces
{
    public interface ICommand : IXmlExportable
    {
        IReadOnlyCommand ToReadOnly();
    }

    public interface ICommand<TCommandBase> : ICommand
        where TCommandBase : ICommand<TCommandBase>
    {
        IEnumerable<TCommandBase> Then(params TCommandBase[] commands);
    }
}