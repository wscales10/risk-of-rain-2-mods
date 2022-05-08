using System;
using System.Collections.Generic;
using Utils;
using Utils.Reflection.Properties;

namespace MyRoR2
{
    public sealed class DefinedScene : MyScene
    {
        private DefinedScene(string displayName, string name) : base(name) => DisplayName = displayName;

        public string DisplayName { get; }

        public static implicit operator DefinedScene((string displayName, string name) pair)
            => new DefinedScene(HelperMethods.AddSpacesToPascalCaseString(pair.displayName), pair.name);

        public static IEnumerable<DefinedScene> GetAll(Predicate<DefinedScene> predicate = null) => typeof(Scenes).GetStaticPropertyValues(predicate);

        public static DefinedScene Get(Predicate<DefinedScene> predicate = null) => typeof(Scenes).GetStaticPropertyValue(predicate);

        public override string ToString() => HelperMethods.GetNullSafeString(DisplayName);
    }
}