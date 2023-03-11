using Newtonsoft.Json;
using Utils;
using Utils.Reflection.Properties;

namespace Minecraft
{
	public sealed class Dimension : ResourceLocation, IEquatable<Dimension>
	{
		[JsonConstructor]
		private Dimension(string? @namespace, string? path) : base(@namespace, path)
		{
		}

		public static Dimension Overworld { get; } = new Dimension("minecraft", "overworld");

		public static Dimension Nether { get; } = new Dimension("minecraft", "the_nether");

		public static Dimension End { get; } = new Dimension("minecraft", "the_end");

		public static IEnumerable<Dimension> GetAll(Predicate<Dimension>? predicate = null) => typeof(Dimension).GetStaticPropertyValues(predicate);

		public static Dimension Get(Predicate<Dimension>? predicate = null) => typeof(Dimension).GetStaticPropertyValue(predicate);

		public static bool operator ==(Dimension? d1, Dimension? d2) => d1 is null ? d2 is null : d1.Equals(d2);

		public static bool operator !=(Dimension? d1, Dimension? d2) => !(d1 == d2);

		public override bool Equals(object? obj) => Equals(obj as Dimension);

		public bool Equals(Dimension? d)
		{
			if (d is null)
			{
				return false;
			}

			if (ReferenceEquals(this, d))
			{
				return true;
			}

			if (GetType() != d.GetType())
			{
				return false;
			}

			return this.PropertywiseEquals(d);
		}

		public override int GetHashCode() => this.MyHashCode();
	}
}