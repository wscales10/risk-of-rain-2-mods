using System.Collections.Generic;
using Utils;

namespace Rules.RuleTypes.Interfaces
{
    public interface IRule<TContext, TOut> : IXmlExportable, ITreeItem<IRule<TContext, TOut>>
    {
        string Name { get; }

        TrackedResponse<TContext, TOut> GetBucket(TContext c);

        TOut GetOutput(TContext newContext);

        IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser);
    }
}