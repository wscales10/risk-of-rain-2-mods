using Spotify.Commands;

namespace Rules.RuleTypes.Interfaces
{
	public interface IBucket : IRule
	{
		ICommandList Commands { get; }
	}
}
