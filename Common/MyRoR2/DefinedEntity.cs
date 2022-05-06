using System;
using System.Collections.Generic;
using Utils;
using Utils.Reflection.Properties;

namespace MyRoR2
{
	public sealed class DefinedEntity : Entity
	{
		private DefinedEntity(string displayName, string name) : base(name) => DisplayName = displayName;

		public string DisplayName { get; }

		public static implicit operator DefinedEntity((string displayName, string name) pair)
			=> new DefinedEntity(HelperMethods.AddSpacesToPascalCaseString(pair.displayName), pair.name);

		public static IEnumerable<DefinedEntity> GetAll(Predicate<DefinedEntity> predicate = null) => typeof(Entities).GetStaticPropertyValues(predicate);

		public static DefinedEntity Get(Predicate<DefinedEntity> predicate = null) => typeof(Entities).GetStaticPropertyValue(predicate);
	}
}