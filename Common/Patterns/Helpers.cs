using System;
using System.Collections.Generic;

namespace Patterns
{
	public static class Helpers
	{
		public static List<TBase> Flatten<TDerived, TBase>(this IEnumerable<TBase> source, Func<TDerived, IEnumerable<TBase>> flattener)
			where TDerived : TBase
		{
			var list = new List<TBase>();
			foreach (TBase b in source)
			{
				if (b is TDerived d)
				{
					list.AddRange(flattener(d));
				}
				else
				{
					list.Add(b);
				}
			}
			return list;
		}
	}
}
