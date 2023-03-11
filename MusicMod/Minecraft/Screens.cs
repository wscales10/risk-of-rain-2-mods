namespace Minecraft
{
	public static class Screens
	{
		public static DefinedScreen MainMenu { get; } = (nameof(MainMenu), "TitleScreen");

		public static DefinedScreen EndPoem { get; } = (nameof(EndPoem), "WinScreen");

		public static DefinedScreen WorldSelect { get; } = (nameof(WorldSelect), "SelectWorldScreen");
	}
}