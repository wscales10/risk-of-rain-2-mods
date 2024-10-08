﻿using Patterns.Patterns.SmallPatterns;
using Patterns.TypeDefs;
using System.Text.RegularExpressions;
using Utils;

namespace MyRoR2
{
	public class ScenePattern : ClassValuePattern<MyScene>
	{
		private DefinedScene scene;

		public string DisplayName => scene?.DisplayName;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<MyScene, ScenePattern>((s) => (ScenePattern)new ScenePattern().DefineWith(s), s => Equals(s));

		public static ScenePattern Equals(MyScene s)
		{
			return (ScenePattern)new ScenePattern().DefineWith(s.Name);
		}

		public void DefineWithDisplayName(string displayName)
		{
			var scene = DefinedScene.Get(s => s.DisplayName == displayName);

			if (!(scene is null))
			{
				DefineWith(scene.Name);
			}
		}

		public override string ToString() => scene?.DisplayName;

		protected override bool isMatch(MyScene value)
		{
			return !(value?.Name is null) && Regex.IsMatch(value.Name, $@"^(IT)?{RegexHelpers.Escape(scene.Name)}(SIMPLE)?\d*$");
		}

		protected override bool defineWith(string stringDefinition)
		{
			var scene = DefinedScene.Get(s => s.Name == stringDefinition);

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
	}
}