using Spotify.Commands;

namespace Rules.RuleTypes.Interfaces
{
    public interface IBucket<TContext> : IRule<TContext>
    {
        ICommandList Commands { get; }
    }
}