using System;

namespace Patterns.TypeDefs
{
	public class TypeDef : TypeDefBase, ITypeDefGetter
	{
		private readonly TypedEqualizer equalizer;

		internal TypeDef(Definer definer, TypedEqualizer equalizer, Type type, PatternTypeGetter patternTypeGetter) : base(definer, type, patternTypeGetter)
		{
			this.equalizer = equalizer;
		}

		internal TypeDef(Definer definer, UntypedEqualizer equalizer, Type type, PatternTypeGetter patternTypeGetter) : base(definer, type, patternTypeGetter)
		{
			this.equalizer = (_, o) => equalizer(o);
		}

		public override IPattern Equalizer(object value) => equalizer(value?.GetType(), value);

		public override IPattern Equalizer<T>(object value) => equalizer(typeof(T), value);

		public override IPattern Equalizer(Type type, object value) => equalizer(type, value);

		public static TypeDef Create<T, TPattern>(Definer definer, TypedEqualizer equalizer)
			where TPattern : IPattern<T>
		{
			return new TypeDef(definer, equalizer, typeof(T), (_) => typeof(TPattern));
		}

		public static TypeDef Create<T, TPattern>(Definer definer, GenericEqualizer<T> equalizer)
			where TPattern : IPattern<T>
		{
			return new TypeDef(definer, (t, o) => equalizer((T)o), typeof(T), (_) => typeof(TPattern));
		}

		public static TypeDef Create<T, TPattern>(Definer definer, UntypedEqualizer equalizer)
			where TPattern : IPattern<T>
		{
			return new TypeDef(definer, equalizer, typeof(T), (_) => typeof(TPattern));
		}

		public TypeDef GetTypeDef(params Type[] genericTypeArgs)
		{
			return genericTypeArgs.Length == 0 ? this : throw new ArgumentOutOfRangeException();
		}
	}
}
