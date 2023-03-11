using Utils;
using Utils.Reflection.Properties;

namespace Minecraft
{
	public sealed class DefinedScreen : IScreen
	{
		private DefinedScreen(string displayName, string name)
		{
			DisplayName = displayName;
			Name = name;
		}

		public string? Name { get; }

		public string? DisplayName { get; }

		public static implicit operator DefinedScreen((string displayName, string name) pair)
			=> new(HelperMethods.AddSpacesToPascalCaseString(pair.displayName), pair.name);

		public static IEnumerable<DefinedScreen> GetAll(Predicate<DefinedScreen>? predicate = null) => typeof(Screens).GetStaticPropertyValues(predicate);

		public static DefinedScreen Get(Predicate<DefinedScreen>? predicate = null) => typeof(Screens).GetStaticPropertyValue(predicate);

		public override string ToString() => HelperMethods.GetNullSafeString(DisplayName);
	}
}