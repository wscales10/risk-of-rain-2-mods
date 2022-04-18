using MyRoR2;

namespace Rules.RuleTypes.Mutable
{
	public abstract class UpperRule : Rule
	{
		public abstract Rule GetRule(Context c);

		public sealed override Bucket GetBucket(Context c) => GetRule(c)?.GetBucket(c);
	}
}
