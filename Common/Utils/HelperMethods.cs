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

				if (char.IsUpper(current) && (char.IsLower(previous) || (char.IsUpper(previous) && char.IsLower(next))))
				{
					list.Add(' ');
				}

				list.Add(current);
			}

			return string.Concat(list);
		}
	}
}