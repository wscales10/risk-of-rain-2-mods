using Spotify.Commands;
using Utils;

namespace Rules.RuleTypes.Interfaces
{
    public interface IRule<TContext> : IXmlExportable
    {
        string Name { get; }

        TrackedResponse<TContext> GetBucket(TContext c);

        ICommandList GetCommands(TContext oldContext, TContext newContext, bool force = false);

        IReadOnlyRule<TContext> ToReadOnly();
    }
}