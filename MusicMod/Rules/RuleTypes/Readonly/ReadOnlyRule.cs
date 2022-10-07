using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Xml.Linq;

namespace Rules.RuleTypes.Readonly
{
    public abstract class ReadOnlyRule<T, TContext, TOut> : IReadOnlyRule<TContext, TOut> where T : Rule<TContext, TOut>
    {
        protected readonly T mutable;

        protected ReadOnlyRule(T mutable, RuleParser<TContext, TOut> ruleParser) => this.mutable = (T)ruleParser.DeepClone(mutable);

        public string Name => mutable.Name;

        public abstract TrackedResponse<TContext, TOut> GetBucket(TContext c);

        // TODO: make GetCommands actually use this object's properties and mutable's methods or whatever
        public TOut GetCommands(TContext oldContext, TContext newContext, bool force = false) => Rule<TContext, TOut>.GetCommands(this, oldContext, newContext, force);

        public IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser) => this;

        public sealed override string ToString() => Name ?? GetType().Name;

        public XElement ToXml() => mutable.ToXml();
    }
}