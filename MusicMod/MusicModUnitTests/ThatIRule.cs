using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyRoR2;
using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Utils;
using static Rules.Examples;
using static Rules.RuleTypes.Mutable.Case;

namespace MusicModUnitTests
{
	[TestClass]
	public class ThatIRule
	{
		[TestMethod]
		public void TestMethod1()
		{
			var rule = MimicRule;
			var oldContext = new Context { SceneName = Scenes.IntroCutscene };
			var newContext = new Context { SceneName = Scenes.MainMenu };
			var commands = rule.GetCommands(oldContext, newContext);
			Assert.IsTrue(commands?.Count() > 0);
		}

		[TestMethod]
		public void TestMethod2()
		{
			var rule = MimicRule;
			var oldContext = new Context { SceneName = Scenes.SimulacrumMenu, RunType = RunType.Simulacrum };
			var newContext = new Context { SceneName = Scenes.CharacterSelect, RunType = RunType.Simulacrum };
			var commands1 = rule.GetCommands(oldContext, newContext);
			var commands2 = rule.GetCommands(newContext, oldContext);
		}

		[TestMethod]
		public void TestMethod3()
		{
			var rule = MimicRule;
			var oldContext = new Context { SceneName = Scenes.SplashScreen };
			var newContext = new Context { SceneName = Scenes.IntroCutscene };
			var commands = rule.GetCommands(oldContext, newContext);
			Assert.IsFalse(commands.Any());
		}

		[TestMethod]
		public void TestMethod4()
		{
			var rule = MimicRule;
			var oldContext = new Context { SceneName = Scenes.MainMenu };
			var newContext = new Context { SceneName = Scenes.CharacterSelect };
			var commands = rule.GetCommands(oldContext, newContext);
			Assert.IsFalse(commands.Any());
		}

		[TestMethod]
		public void TestMethod5()
		{
			var rule = MimicRule;
			var oldContext = new Context { SceneName = Scenes.DistantRoost };
			var newContext = new Context { SceneName = Scenes.BazaarBetweenTime };
			var commands = rule.GetCommands(oldContext, newContext);
		}

		[TestMethod]
		public void ThatMimicRuleXmlMatches()
		{
			var mimicRule = MimicRule;
			var xml = mimicRule.ToXml();
			string originalXml = Path.Combine(Paths.AssemblyDirectory, "original.xml");
			xml.Save(originalXml);
			var customRule = Rule.FromXml(xml);
			string parsedXml = Path.Combine(Paths.AssemblyDirectory, "parsed.xml");
			customRule.ToXml().Save(parsedXml);
			Assert.AreEqual(File.ReadAllText(originalXml), File.ReadAllText(parsedXml));
		}

		[TestMethod]
		public void ThatNullableIntRuleXmlMatches()
		{
			var newRule = new SwitchRule<int?>(nameof(Context.LoopIndex), C(new PauseCommand(), (int?)null), C<int?>(new ResumeCommand(), 1));
			var xml = newRule.ToXml();
			string originalXml = Path.Combine(Paths.AssemblyDirectory, "original.xml");
			xml.Save(originalXml);
			var customRule = Rule.FromXml(xml);
			string parsedXml = Path.Combine(Paths.AssemblyDirectory, "parsed.xml");
			customRule.ToXml().Save(parsedXml);
			Assert.AreEqual(File.ReadAllText(originalXml), File.ReadAllText(parsedXml));
			var menu = new Context { SceneType = RoR2.SceneType.Menu, SceneName = Scenes.MainMenu };
			var stage = new Context { SceneType = RoR2.SceneType.Stage, SceneName = Scenes.DistantRoost, LoopIndex = 1 };
			Debugger.Break();
		}

		[TestMethod]
		public void ThatTimeSpanToCompactStringWorks()
		{
			var ts1 = TimeSpan.FromSeconds(-23);
			Assert.AreEqual("-23s", ts1.ToCompactString());
		}

		[TestMethod]
		public void ThatSimplifyOrWorks()
		{
			var x = IntPattern.x;

			var ip1 = x < 4;
			var ip2 = 3 < x <= 8;
			var ip3 = 10 <= x <= 11;
			var ip4 = 12 <= x < 20;
			var ip5 = x > 100;

			var op = new OrPattern<int>(ip1, ip2, ip3, ip4, ip5);
			var simplified = op.Simplify();
			var expected = (x < 9) | (9 < x <= 19) | (x > 100);
			Assert.AreEqual(simplified.ToString(), expected.ToString());
		}
	}
}