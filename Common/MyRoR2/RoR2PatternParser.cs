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
			if (typeKey == nameof(MyScene))
			{
				typeDefGetter = ScenePattern.TypeDef;
				return true;
			}
			else
			{
				return base.TryGetTypeDefGetter(typeKey, out typeDefGetter);
			}
		}
	}
}
