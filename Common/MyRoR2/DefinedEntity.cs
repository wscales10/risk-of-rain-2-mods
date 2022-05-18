using System;
using System.Collections.Generic;
using Utils;
using Utils.Reflection.Properties;

namespace MyRoR2
{
    public sealed class DefinedEntity : Entity
    {
        private DefinedEntity(string displayName, string name) : this(displayName, name, displayName)
        {
        }

        private DefinedEntity(string displayName, string name, string typeableName) : base(name)
        {
            DisplayName = displayName;
            TypeableName = typeableName;
        }

        public string DisplayName { get; }

        public string TypeableName { get; }

        public static implicit operator DefinedEntity((string displayName, string name) tuple)
            => new DefinedEntity(HelperMethods.AddSpacesToPascalCaseString(tuple.displayName), tuple.name);

        public static implicit operator DefinedEntity((string displayName, string name, string typeableName) tuple)
            => new DefinedEntity(HelperMethods.AddSpacesToPascalCaseString(tuple.displayName), tuple.name, tuple.typeableName);

        public static IEnumerable<DefinedEntity> GetAll(Predicate<DefinedEntity> predicate = null) => typeof(Entities).GetStaticPropertyValues(predicate);

        public static DefinedEntity Get(Predicate<DefinedEntity> predicate = null) => typeof(Entities).GetStaticPropertyValue(predicate);
    }
}