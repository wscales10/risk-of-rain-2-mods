using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Patterns.Patterns
{
	internal static class HelperMethods
	{
		private static List<(int, int)> CreatePairs(int count)
		{
			var pairs = new List<(int, int)>();

			for (int i = 0; i < count - 1; i++)
			{
				for (int j = i + 1; j < count; j++)
				{
					pairs.Add((i, j));
				}
			}

			return pairs;
		}

		internal static List<IPattern<T>> Simplify<T>(IList<IPattern<T>> source, Func<IPattern<T>, IPattern<T>, IPattern<T>> simplifier)
		{
			int i, j, c = 0, k = source.Count;

			var dict = source.ToDictionary();

			var pairs = CreatePairs(source.Count);

			while (c < pairs.Count)
			{
				(i, j) = pairs[c];
				var p1 = dict[i];
				var p2 = dict[j];

				var simplified = simplifier(p1, p2) ?? simplifier(p2, p1);

				if (!(simplified is null))
				{
					dict.Remove(i);
					dict.Remove(j);

					for(int l = 0; l < pairs.Count; l++)
					{
						var pair = pairs[l];

						if (pair.Item1 == i || pair.Item2 == i || pair.Item1 == j || pair.Item2 == j)
						{
							if (l <= c)
							{
								c--;
							}

							pairs.RemoveAt(l--);
						}
					}

					foreach (var key in dict.Keys)
					{
						pairs.Add((key, k));
					}

					dict[k++] = simplified;
				}

				c++;
			}

			return dict.Values.ToList();
		}
	}
}
