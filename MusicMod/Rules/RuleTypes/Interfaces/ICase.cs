using MyRoR2;
using Patterns;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
	public interface ICase<T>
	{
		IPattern<Context> WherePattern { get; }

		IEnumerable<T> Arr { get; }

		IRule Output { get; }
		string Name { get; }
	}

	public interface ICaseGetter<T>
	{
		IEnumerable<Case<T>> GetCases();
	}
}
