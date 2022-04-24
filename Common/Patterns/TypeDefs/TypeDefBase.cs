using System;
using System.Reflection;
using System.Xml.Linq;
using Utils.Reflection;

namespace Patterns.TypeDefs
{
	public delegate Type PatternTypeGetter(Type valueType);

	public abstract class TypeDefBase
	{
		private readonly MethodInfo parser;

		protected TypeDefBase(Definer definer, Type type, PatternTypeGetter patternTypeGetter)
		{
			Definer = definer;
			Type = type;
			PatternTypeGetter = patternTypeGetter;
			parser = typeof(PatternParser).GetMethod(nameof(PatternParser.Parse), mi => mi.IsGenericMethod).MakeGenericMethod(Type);
		}

		public Definer Definer { get; }

		public Type Type { get; }

		public PatternTypeGetter PatternTypeGetter { get; }

		public abstract IPattern Equalizer(object value);

		public abstract IPattern Equalizer<T>(object value);

		public abstract IPattern Equalizer(Type type, object value);

		public Func<XElement, IPattern> GetParser(PatternParser patternParser)
		{
			return e => (PatternBase)parser.Invoke(patternParser, new[] { e });
		}
	}
}