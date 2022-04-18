using Utils;

namespace MyRoR2
{
	public sealed class DefinedScene : MyScene
	{
		private DefinedScene(string displayName, string name) : base(name) => DisplayName = displayName;

		public string DisplayName { get; }

		public static implicit operator DefinedScene((string displayName, string name) pair)
			=> new DefinedScene(HelperMethods.AddSpacesToPascalCaseString(pair.displayName), pair.name);
	}
}