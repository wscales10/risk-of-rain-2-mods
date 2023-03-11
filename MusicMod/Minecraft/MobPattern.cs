using Patterns.Patterns.SmallPatterns;
using Patterns.TypeDefs;
using System.Text.RegularExpressions;
using Utils;

namespace Minecraft
{
	public class MobPattern : ClassValuePattern<Mob>
	{
		private DefinedMob? mob;

		public string? DisplayName => mob?.DisplayName;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<Mob, MobPattern>((s) => (MobPattern)new MobPattern().DefineWith(s), s => Equals(s));

		public static MobPattern Equals(Mob m)
		{
			return (MobPattern)new MobPattern().DefineWith(m.Id);
		}

		public void DefineWithDisplayName(string displayName)
		{
			var definedMob = DefinedMob.Get(s => s.DisplayName == displayName);

			if (definedMob is not null)
			{
				DefineWith(definedMob.Id);
			}
		}

		public override string ToString() => HelperMethods.GetNullSafeString(mob?.DisplayName);

		protected override bool isMatch(Mob value)
		{
			return value?.Id is not null && Regex.IsMatch(value.Id, $"^{RegexHelpers.Escape(mob?.Id)}2?$");
		}

		protected override bool defineWith(string stringDefinition)
		{
			var definedMob = DefinedMob.Get(s => s.Id == stringDefinition);

			if (definedMob is null)
			{
				return false;
			}
			else
			{
				mob = definedMob;
				return true;
			}
		}
	}
}