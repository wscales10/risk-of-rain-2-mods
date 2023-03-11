using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuleExamples.RiskOfRain2;

namespace ContextModUnitTests
{
	[TestClass]
	public class ThatRule
	{
		[TestMethod]
		public void TestMethod1()
		{
			var rule = MimicRule.Instance;
			var bucket = rule.GetBucket("Main Menu");
			System.Diagnostics.Debugger.Break();
		}
	}
}