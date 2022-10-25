using System;

namespace Patterns.Patterns
{
	public static class Query<TObject>
	{
		public static PropertyPattern<TObject> Create<T>(string propertyName, IPattern pattern)
		{
			if (pattern is IPattern<T> typedPattern)
			{
				return Create(propertyName, typedPattern);
			}

			return Create(typeof(T), propertyName, pattern);
		}

		public static PropertyPattern<TObject> Create<T>(string propertyName, IPattern<T> pattern) => Create(typeof(T), propertyName, pattern.Simplify());

		public static PropertyPattern<TObject> Create(Type type, string propertyName, IPattern pattern) => new PropertyPattern<TObject>(propertyName, type, pattern);
	}
}