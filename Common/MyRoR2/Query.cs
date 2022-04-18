using Patterns;
using Patterns.Patterns;
using System;

namespace MyRoR2
{
	public class Query : PropertyPattern<Context>
	{
		protected Query(string propertyName, Type propertyType, IPattern pattern) : base(propertyName, propertyType, pattern)
		{
		}
	}
}
