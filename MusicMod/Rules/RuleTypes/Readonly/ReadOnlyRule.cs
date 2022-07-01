using Rules.RuleTypes.Interfaces;
using Spotify.Commands;
using System.Xml.Linq;

namespace Rules.RuleTypes.Readonly
{
    public abstract class ReadOnlyRule<T, TContext> : IReadOnlyRule<TContext> where T : Mutable.Rule<TContext>
    {
        private readonly T mutable;

        protected ReadOnlyRule(T mutable) => this.mutable = mutable;

        public string Name => mutable.Name;

        public TrackedResponse<TContext> GetBucket(TContext c) => mutable.GetBucket(c).ToReadOnly();

        public ICommandList GetCommands(TContext oldContext, TContext newContext, bool force = false)
        {
            var output = mutable.GetCommands(oldContext, newContext, force);

            if (output is CommandList commandList)
            {
                return commandList.ToReadOnly();
            }
            else
            {
                return output;
            }
        }

        public IReadOnlyRule<TContext> ToReadOnly() => this;

        public sealed override string ToString() => Name ?? GetType().Name;

        public XElement ToXml() => mutable.ToXml();
    }
}