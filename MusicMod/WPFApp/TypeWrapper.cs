using System;
using Utils;

namespace WPFApp
{
	public class TypeWrapper
	{
		public TypeWrapper(Type type) => Type = type;

		public Type Type { get; }

		public string DisplayName => Type.GetDisplayName();

		public static implicit operator TypeWrapper(Type type) => new(type);

		public static implicit operator Named<object>(TypeWrapper typeWrapper) => new(typeWrapper.DisplayName, typeWrapper.Type);
	}
}