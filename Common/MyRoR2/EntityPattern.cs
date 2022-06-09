using Patterns.Patterns.SmallPatterns;
using Patterns.TypeDefs;
using System.Text.RegularExpressions;

namespace MyRoR2
{
    public class EntityPattern : ClassValuePattern<Entity>
    {
        private DefinedEntity entity;

        public string DisplayName => entity?.DisplayName;

        internal static TypeDef TypeDef { get; } = TypeDef.Create<Entity, EntityPattern>((s) => (EntityPattern)new EntityPattern().DefineWith(s), s => Equals(s));

        public static EntityPattern Equals(Entity s)
        {
            return (EntityPattern)new EntityPattern().DefineWith(s.Name);
        }

        public void DefineWithDisplayName(string displayName)
        {
            var entity = DefinedEntity.Get(s => s.DisplayName == displayName);

            if (!(entity is null))
            {
                DefineWith(entity.Name);
            }
        }

        public override string ToString() => entity?.DisplayName;

        protected override bool isMatch(Entity value)
        {
            return !(value?.Name is null) && Regex.IsMatch(value.Name, $"^{Regex.Escape(entity.Name)}2?$");
        }

        protected override bool defineWith(string stringDefinition)
        {
            var entity = DefinedEntity.Get(s => s.Name == stringDefinition);

            if (entity is null)
            {
                return false;
            }
            else
            {
                this.entity = entity;
                return true;
            }
        }
    }
}