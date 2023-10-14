using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rules.RuleTypes.Readonly
{
    public abstract class ReadOnlyRule<T, TContext, TOut> : IReadOnlyRule<TContext, TOut> where T : Rule<TContext, TOut>
    {
        protected readonly T mutable;

        protected ReadOnlyRule(T mutable, RuleParser<TContext, TOut> ruleParser) => this.mutable = (T)ruleParser.DeepClone(mutable);

        public string Name => mutable.Name;

        public virtual IEnumerable<(string, IRule<TContext, TOut>)> Children => Rule.GetChildren(this);

        public abstract TrackedResponse<TContext, TOut> GetBucket(TContext c);

        // TODO: make GetCommands actually use this object's properties and mutable's methods or whatever
        public TOut GetOutput(TContext newContext) => Rule<TContext, TOut>.GetOutput(this, newContext);

        public IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser) => this;

        public sealed override string ToString() => Name ?? GetType().Name;

        public XElement ToXml() => mutable.ToXml();
    }
}