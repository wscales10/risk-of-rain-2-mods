using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utils.Tests
{
	[TestClass]
	public class ThatRegexHelpers
	{
		[TestMethod]
		[DataRow("Main Menu", "Main Menu")]
		[DataRow(@" \. .", @" \\\. \.")]
		[DataRow(@"\ .\ ", @"\\ \.\\ ")]
		[DataRow(@"\.\ \", @"\\\.\\ \\")]
		[DataRow("cJuE5  2LW", @"cJuE5  2LW")]
		[DataRow("*Yxk5STL)+", @"\*Yxk5STL\)\+")]
		public void Escape_FunctionsProperly(string input, string expectedOutput)
		{
			string actualOutput = RegexHelpers.Escape(input);
			Assert.AreEqual(expectedOutput, actualOutput);
		}
	}
}