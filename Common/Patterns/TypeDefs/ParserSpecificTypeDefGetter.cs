using System;

namespace Patterns.TypeDefs
{

	internal class ParserSpecificTypeDefGetter
	{
		private readonly Func<PatternParser, ITypeDefGetter> func;

		public ParserSpecificTypeDefGetter(Func<PatternParser, ITypeDefGetter> func)
		{
			this.func = func;
		}

		public ITypeDefGetter Get(PatternParser patternParser)
		{
			return func(patternParser);
		}
	}
}
