using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Utils;

namespace MusicModUnitTests
{
	[TestClass]
	public class ActualProgram
	{
		private int index = 0;

		[TestMethod]
		public async Task RunAsync()
		{
			for (int i = 0; i < 10; i++)
			{
				_ = TaskScheduler.Default;
				await Task.Delay(1000);
			}
		}

		public async Task DoStuffAsync()
		{
			int id = ++index;
			this.Log("starting " + id);
			await Task.Delay(10000);
			this.Log("finished " + id);
		}
	}
}
