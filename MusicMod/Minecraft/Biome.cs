using Utils;
using Utils.Reflection.Properties;

namespace Minecraft
{
	public sealed class Biome : ResourceLocation, IEquatable<Biome>
	{
		public Biome(string? @namespace, string? path) : base(@namespace, path)
		{
			Base = this;
		}

		private Biome(Biome @base, string? @namespace, string? path) : this(@namespace, path)
		{
			Base = @base;
		}

		public Biome Base { get; }

		public static IEnumerable<Biome> GetAll(Predicate<Biome>? predicate = null) => typeof(Biomes).GetStaticPropertyValues(predicate).Concat(typeof(Biomes).GetNestedTypes().SelectMany(b => b.GetStaticPropertyValues(predicate)));

		public static Biome? Get(Predicate<Biome>? predicate = null) => GetAll(predicate).Single();

		public static bool TryGet(out Biome? biome) => TryGet(null, out biome);

		public static bool TryGet(Predicate<Biome>? predicate, out Biome? biome)
		{
			biome = GetAll(predicate).SingleOrDefault();
			return biome != null;
		}

		public static bool operator ==(Biome? b1, Biome? b2) => b1 is null ? b2 is null : b1.Equals(b2);

		public static bool operator !=(Biome? b1, Biome? b2) => !(b1 == b2);

		public override string ToString() => $"{Base?.AsString()} / {base.ToString()}";

		public Biome Variant(string? path) => Variant(Namespace, path);

		public Biome Variant(string? @namespace, string? path) => new(this, @namespace, path);

		public override bool Equals(object? obj) => Equals(obj as Biome);

		public bool Equals(Biome? b)
		{
			if (b is null)
			{
				return false;
			}

			if (ReferenceEquals(this, b))
			{
				return true;
			}

			if (GetType() != b.GetType())
			{
				return false;
			}

			return this.Namespace == b.Namespace && this.Path == b.Path && this.Base.ToString() == b.Base.ToString();
		}

		public override int GetHashCode() => this.MyHashCode();
	}
}