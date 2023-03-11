using Newtonsoft.Json;
using Utils;
using Utils.Reflection.Properties;

namespace Minecraft
{
	public struct MinecraftContext : IEquatable<MinecraftContext>
	{
		public Biome Biome { get; set; }

		public IList<Mob> Bosses { get; set; }

		public Dimension Dimension { get; set; }

		public GameMode? GameMode { get; set; }

		public bool IsUnderwater { get; set; }

		[JsonConverter(typeof(ScreenJsonConverter))]
		public IScreen Screen { get; set; }

		public static bool operator ==(MinecraftContext c1, MinecraftContext c2) => c1.Equals(c2);

		public static bool operator !=(MinecraftContext c1, MinecraftContext c2) => !c1.Equals(c2);

		public override bool Equals(object? obj) => obj is MinecraftContext c && Equals(c);

		public bool Equals(MinecraftContext c) => this.PropertywiseEquals(c);

		public override int GetHashCode() => this.MyHashCode();
	}
}