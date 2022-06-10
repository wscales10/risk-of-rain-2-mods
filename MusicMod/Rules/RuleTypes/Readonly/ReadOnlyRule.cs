using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Spotify.Commands.Interfaces;
using Spotify.Commands.Mutable;
using System.Xml.Linq;

namespace Rules.RuleTypes.Readonly
{
    public abstract class ReadOnlyRule<T> : IReadOnlyRule where T : Mutable.Rule
    {
        private readonly T mutable;

        protected ReadOnlyRule(T mutable) => this.mutable = mutable;

        public string Name => mutable.Name;

        public TrackedResponse GetBucket(Context c) => mutable.GetBucket(c).ToReadOnly();

        public ICommandList GetCommands(Context oldContext, Context newContext, bool force = false)
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

        public IReadOnlyRule ToReadOnly() => this;

        public sealed override string ToString() => Name ?? GetType().Name;

        public XElement ToXml() => mutable.ToXml();
    }
}