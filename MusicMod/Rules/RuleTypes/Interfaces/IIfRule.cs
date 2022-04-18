using MyRoR2;
using Patterns;

namespace Rules.RuleTypes.Interfaces
{
	public interface IIfRule : IRule
	{
		IPattern<Context> Pattern { get; }

		IRule ThenRule { get; }

		IRule ElseRule { get; }
	}
}
