﻿using System;
using System.Diagnostics;

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

		public override IPattern Equalizer(object value) => equalizer(value?.GetType(), value);

		public override IPattern Equalizer<T>(object value) => equalizer(typeof(T), value);

		public override IPattern Equalizer(Type type, object value) => equalizer(type, value);

		public TypeDef GetTypeDef(TypeRef typeRef)
		{
			if (typeRef.GenericSize == 0)
			{
				return this;
			}
			else
			{
				Debugger.Break();
				throw new ArgumentOutOfRangeException(nameof(typeRef), typeRef, "TypeDef cannot handle generics");
			}
		}
	}
}