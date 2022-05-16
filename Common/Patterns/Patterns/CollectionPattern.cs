using Patterns.TypeDefs;
using System;
using System.Collections.Generic;

namespace Patterns.Patterns
{
    public static class CollectionPattern
    {
        internal static ParserSpecificTypeDefGetter GenericTypeDef { get; }
            = new ParserSpecificTypeDefGetter(
                patternParser => new BestTypeDefGetter(
                    typeRef =>
                    {
                        return new TypeDef(
                            (_) => throw new InvalidOperationException(),
                            (t, x) => throw new NotSupportedException(),
                            typeof(IList<>).MakeGenericType(typeRef.GenericTypeArguments),
                            t => typeof(CollectionPattern<>).MakeGenericType(t.GenericTypeArguments));
                    }));
    }

    public abstract class CollectionPattern<T> : Pattern<IList<T>>
    {
    }
}