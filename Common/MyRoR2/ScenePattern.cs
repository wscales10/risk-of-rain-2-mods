using Patterns.Patterns.SmallPatterns;
using Patterns.TypeDefs;
using System;
using System.Text.RegularExpressions;
using Utils;

namespace MyRoR2
{
	public class ScenePattern : ClassValuePattern<MyScene>
	{
		private DefinedScene scene;

		public string DisplayName => scene?.DisplayName;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<MyScene, ScenePattern>((s, _, __) => (ScenePattern)new ScenePattern().DefineWith(s), s => Equals(s));

		public static ScenePattern Equals(MyScene s)
		{
			return (ScenePattern)new ScenePattern().DefineWith(s.Name);
		}

		protected override bool isMatch(MyScene value)
		{
			return !(value is null) && Regex.IsMatch(value.Name, $"^(IT)?{Regex.Escape(scene.Name)}(SIMPLE)?2?$");
		}

		protected override bool defineWith(string stringDefinition)
		{
			var scene = typeof(Scenes).GetStaticPropertyValue((Predicate<DefinedScene>)(s => s.Name == stringDefinition));

			if (scene is null)
			{
				return false;
			}
			else
			{
				this.scene = scene;
				return true;
			}
		}

		public void DefineWithDisplayName(string displayName)
		{
			var scene = typeof(Scenes).GetStaticPropertyValue((Predicate<DefinedScene>)(s => s.DisplayName == displayName));

			if (!(scene is null))
			{
				DefineWith(scene.Name);
			}
		}
	}
}