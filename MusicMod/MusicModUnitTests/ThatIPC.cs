using Microsoft.VisualStudio.TestTools.UnitTesting;
using IPC;
using System.Collections.Generic;
using System.Threading;
using Utils;

namespace MusicModUnitTests
{
	[TestClass]
	public class ThatIPC
	{
		[TestMethod]
		public void TestMethod1()
		{
			var server = new Server(5007);
			server.TryStart.CreateRun().RunToCompletion();

			var client = new Client(5007);
			client.TryStart.CreateRun().RunToCompletion();

			server.ReceivedRequest += Server_ReceivedRequest;
			client.ReceivedRequest += Client_ReceivedRequest;

			Thread.Sleep(5000);

			server.Broadcast(new Message("msg", "server to client message"), new Message("msg", "stc line 2"));

			Thread.Sleep(2000);

			client.SendToServer(new Message("msg", "client to server message"), new Message("msg", "cts line 2"));

			Thread.Sleep(10000);
		}

		private IEnumerable<Message> Client_ReceivedRequest(IEnumerable<Message> arg)
		{
			this.Log("Server sent to client:");

			foreach (var line in arg)
			{
				this.Log(line);
				yield return new Message("fuck");
			}
		}

		private IEnumerable<Message> Server_ReceivedRequest(IEnumerable<Message> arg)
		{
			this.Log("Client sent to server:");

			foreach (var line in arg)
			{
				this.Log(line);
				yield return new Message("fuck");
			}
		}
	}
}