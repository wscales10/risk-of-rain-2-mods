using Utils;
using Utils.Reflection.Properties;

namespace Minecraft
{
	public class DefinedMob : Mob
	{
		internal DefinedMob(string displayName, string id) : base(id)
		{
			DisplayName = displayName;
		}

		public string DisplayName { get; }

		public static implicit operator DefinedMob((string displayName, string id) pair)
			=> new(HelperMethods.AddSpacesToPascalCaseString(pair.displayName), pair.id);

		public static IEnumerable<DefinedMob> GetAll(Predicate<DefinedMob>? predicate = null) => typeof(Mobs).GetStaticPropertyValues(predicate);

		public static DefinedMob Get(Predicate<DefinedMob>? predicate = null) => typeof(Mobs).GetStaticPropertyValue(predicate);

		public override string ToString() => HelperMethods.GetNullSafeString(DisplayName);
	}
}