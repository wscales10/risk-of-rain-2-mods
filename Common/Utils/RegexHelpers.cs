namespace Utils
{
	public static class RegexHelpers
	{
		public static string Escape(string str) => System.Text.RegularExpressions.Regex.Escape(str).Replace("\\ ", " ");
	}
}