using Patterns;
using Patterns.TypeDefs;

namespace MyRoR2
{
	public class RoR2PatternParser : PatternParser
	{
		protected RoR2PatternParser()
		{
		}

		public static new RoR2PatternParser Instance { get; } = new RoR2PatternParser();

		protected override bool TryGetTypeDefGetter(string typeKey, out ITypeDefGetter typeDefGetter)
		{
			switch (typeKey)
			{
				case nameof(MyScene):
					typeDefGetter = ScenePattern.TypeDef;
					return true;

				case nameof(Entity):
					typeDefGetter = EntityPattern.TypeDef;
					return true;

				default:
					return base.TryGetTypeDefGetter(typeKey, out typeDefGetter);
			}
		}
	}
}