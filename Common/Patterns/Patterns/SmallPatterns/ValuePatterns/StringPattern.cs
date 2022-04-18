using Patterns.TypeDefs;
using System.Text.RegularExpressions;

namespace Patterns.Patterns.SmallPatterns.ValuePatterns
{
	public class StringPattern : ClassValuePattern<string>
	{
		internal static TypeDef TypeDef { get; } = TypeDef.Create<string, StringPattern>((s, _, __)  => (StringPattern)new StringPattern().DefineWith(s), s => Equals(s));

		private Regex regex;

		protected override bool isMatch(string value) => regex.IsMatch(value);

		public override bool IsMatch(object value) => IsMatch(value?.ToString());

		protected override bool defineWith(string stringDefinition)
		{
			regex = new Regex(stringDefinition);
			return true;
		}

		public static StringPattern Equals(string s)
		{
			return (StringPattern)new StringPattern().DefineWith(Regex.Escape(s));
		}
	}
}
