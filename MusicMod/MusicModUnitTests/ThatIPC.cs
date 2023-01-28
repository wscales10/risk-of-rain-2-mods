using Microsoft.VisualStudio.TestTools.UnitTesting;
using IPC;
using System.Collections.Generic;
using System.Threading;
using Utils;
using System;
using MyRoR2;
using System.Diagnostics;
using System.Linq;

namespace MusicModUnitTests
{
	[TestClass]
	public class ThatIPC
	{
		[TestMethod]
		public void TestServer()
		{
			var server = new Server(5008);
			server.TryStart.CreateRun().RunToCompletion(true);

			for (int i = 0; ; i++)
			{
				Thread.Sleep(5000);
				server.Broadcast(new IPC.Message("pause"));
				Thread.Sleep(5000);
				server.Broadcast(new IPC.Message("resume"));
			}
		}

		[TestMethod]
		public void TestClient()
		{
			var client = new Client(5008);
			client.ReceivedRequest += Client_ReceivedRequest1;
			client.TryStart.CreateRun().RunToCompletion(true);
			Thread.Sleep(60000);
		}

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

			client.SendToServerAwaitResponse(new Message("msg", "client to server message"), new Message("msg", "cts line 2"));

			Thread.Sleep(10000);
		}

		[TestMethod]
		public void TestMethod2()
		{
			Server server = new Server(5007);
			Client client = new Client(5007);
			bool tested = false;
			Context expectedContext = new Context { BossBodyName = Entities.Gup, IsBossEncounter = true, LoopIndex = 0, Survivor = Entities.Captain, SceneName = Scenes.SunderedGrove };
			server.ReceivedRequest += arg =>
			{
				foreach (var message in arg)
				{
					if (message.Key == "context")
					{
						var receivedContext = Json.FromJson<Context>(arg.Single().Value);

						foreach (var property in typeof(Context).GetProperties())
						{
							object expectedValue = property.GetValue(expectedContext);
							object actualValue = property.GetValue(receivedContext);
							if (!object.Equals(expectedValue, actualValue))
							{
								Debug.WriteLine($"{property.Name}: {expectedValue} != {actualValue}");
							}
						}

						tested = true;
					}
				}

				return Enumerable.Empty<Message>();
			};
			Assert.IsTrue(server.TryStart.CreateRun().Run(u => !(u.Args is Exception)).Result.Success);
			Assert.IsTrue(client.TryStart.CreateRun().Run(u => !(u.Args is Exception)).Result.Success);
			client.SendToServerAwaitResponse(new Message("context", Json.ToJson(expectedContext)));
			Assert.IsTrue(tested);
		}

		private IEnumerable<Message> Client_ReceivedRequest1(IEnumerable<Message> arg)
		{
			foreach (var message in arg)
			{
				this.Log(message);
			}

			yield break;
		}

		private IEnumerable<Message> Server_ReceivedRequest1(IEnumerable<Message> arg)
		{
			foreach (var message in arg)
			{
				Debug.Write(message);
			}

			yield break;
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