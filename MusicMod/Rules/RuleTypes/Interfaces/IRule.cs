﻿using Utils;

namespace Rules.RuleTypes.Interfaces
{
	public interface IRule<TContext, TOut> : IXmlExportable
	{
		string Name { get; }

		TrackedResponse<TContext, TOut> GetBucket(TContext c);

		TOut GetOutput(TContext oldContext, TContext newContext, bool force = false);

		IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser);
	}
}