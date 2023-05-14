using System.Collections.Generic;

namespace Utils
{
	public static class HelperMethods
	{
		public static string AddSpacesToPascalCaseString(string pascalCaseString)
		{
			var list = new List<char> { pascalCaseString[0] };

			for (int i = 1; i < pascalCaseString.Length; i++)
			{
				var previous = pascalCaseString[i - 1];
				var current = pascalCaseString[i];
				var next = i == pascalCaseString.Length - 1 ? 'A' : pascalCaseString[i + 1];

				if (shouldAddSpace())
				{
					list.Add(' ');
				}

				list.Add(current);

				bool shouldAddSpace()
				{
					// Add spaces before and after numbers
					if (char.IsNumber(current) != char.IsNumber(previous))
					{
						return true;
					}

					// Not the start of a word
					if (!char.IsUpper(current))
					{
						return false;
					}

					// The start of a word
					if (char.IsLower(previous))
					{
						return true;
					}

					if (char.IsUpper(previous) && char.IsLower(next))
					{
						return true;
					}

					return false;
				}
			}

			return string.Concat(list);
		}

		public static string GetNullSafeString(object obj) => obj?.ToString() ?? "null";
	}
}