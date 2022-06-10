using Spotify.Commands.Interfaces;

namespace Rules.RuleTypes.Interfaces
{
    public interface IBucket : IRule
    {
        ICommandList Commands { get; }
    }
}