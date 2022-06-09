using MyRoR2;
using Spotify.Commands;
using Utils;

namespace Rules.RuleTypes.Interfaces
{
    public interface IRule : IXmlExportable
    {
        string Name { get; }

        TrackedResponse GetBucket(Context c);

        ICommandList GetCommands(Context oldContext, Context newContext, bool force = false);

        IReadOnlyRule ToReadOnly();
    }
}