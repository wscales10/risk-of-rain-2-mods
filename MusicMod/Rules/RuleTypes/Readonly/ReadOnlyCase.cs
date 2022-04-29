using MyRoR2;
using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils;

namespace Rules.RuleTypes.Readonly
{
	public class ReadOnlyCase<TValue> : ICase<TValue>
	{
		private readonly string toString;

		public ReadOnlyCase(Case<TValue> mutable)
		{
			toString = mutable.ToString();
			Name = mutable.Name;
			WherePattern = new ReadOnlyPattern<Context>(mutable.WherePattern);
			Arr = mutable.Arr.ToReadOnlyCollection();
			Output = mutable.Output.ToReadOnly();
		}

		public ReadOnlyPattern<Context> WherePattern { get; }

		public ReadOnlyCollection<TValue> Arr { get; }

		public IRule Output { get; }

		public string Name { get; }

		IPattern<Context> ICase<TValue>.WherePattern => WherePattern;

		IEnumerable<TValue> ICase<TValue>.Arr => Arr;

		public override string ToString() => toString;
	}
}