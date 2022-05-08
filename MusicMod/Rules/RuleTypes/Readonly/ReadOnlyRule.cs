using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Spotify.Commands;
using System.Xml.Linq;

namespace Rules.RuleTypes.Readonly
{
	public abstract class ReadOnlyRule<T> : IReadOnlyRule where T : Mutable.Rule
	{
		private readonly T mutable;

		protected ReadOnlyRule(T mutable) => this.mutable = mutable;

		public string Name => mutable.Name;

		public IBucket GetBucket(Context c) => mutable.GetBucket(c);

		public ICommandList GetCommands(Context oldContext, Context newContext, bool force = false) => mutable.GetCommands(oldContext, newContext, force)?.ToReadOnly();

		public IReadOnlyRule ToReadOnly() => this;

		public sealed override string ToString() => Name ?? GetType().Name;

		public XElement ToXml() => mutable.ToXml();
	}
}
