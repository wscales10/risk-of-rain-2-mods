using Patterns.TypeDefs;
using System.Text.RegularExpressions;

namespace Patterns.Patterns.SmallPatterns.ValuePatterns
{
	public class StringPattern : ClassValuePattern<string>
	{
		private Regex regex;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<string, StringPattern>((s, _) => (StringPattern)new StringPattern().DefineWith(s), s => Equals(s));

		public static StringPattern Equals(string s)
		{
			return (StringPattern)new StringPattern().DefineWith(Regex.Escape(s));
		}

		public override bool IsMatch(object value) => IsMatch(value?.ToString());

		protected override bool isMatch(string value) => regex.IsMatch(value);

		protected override bool defineWith(string stringDefinition)
		{
			regex = new Regex(stringDefinition);
			return true;
		}
	}
}