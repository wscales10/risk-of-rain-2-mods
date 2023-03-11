using Patterns.Patterns.SmallPatterns;
using Patterns.TypeDefs;
using Utils;

namespace Minecraft
{
	public class ScreenPattern : ClassValuePattern<IScreen>
	{
		private DefinedScreen? screen;

		public string? DisplayName => screen?.DisplayName;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<IScreen, ScreenPattern>((s) => (ScreenPattern)new ScreenPattern().DefineWith(s), s => Equals(s));

		public static ScreenPattern Equals(IScreen s)
		{
			return (ScreenPattern)new ScreenPattern().DefineWith(s.Name);
		}

		public void DefineWithDisplayName(string displayName)
		{
			var definedScreen = DefinedScreen.Get(s => s.DisplayName == displayName);

			if (definedScreen is not null)
			{
				DefineWith(definedScreen.Name);
			}
		}

		public override string ToString() => HelperMethods.GetNullSafeString(screen?.DisplayName);

		protected override bool isMatch(IScreen value)
		{
			if (screen is null)
			{
				throw new InvalidOperationException();
			}

			return value.Name == screen.Name;
		}

		protected override bool defineWith(string stringDefinition)
		{
			var definedScreen = DefinedScreen.Get(s => s.Name == stringDefinition);

			if (definedScreen is null)
			{
				return false;
			}
			else
			{
				this.screen = definedScreen;
				return true;
			}
		}
	}
}