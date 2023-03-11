using Microsoft.VisualStudio.TestTools.UnitTesting;
using IPC;
using Utils;
using MyRoR2;
using System.Diagnostics;

namespace ContextModUnitTests
{
	[TestClass]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "<Pending>")]
	public class ThatIPC
	{
		[TestMethod]
		public void TestServer()
		{
			var server = new Server(5008, nameof(TestServer));
			server.TryStart.CreateRun().RunToCompletion(true);
			Thread.Sleep(TimeSpan.FromHours(2));
		}

		[TestMethod]
		public void TestClient()
		{
			var client = new Client(5008, nameof(TestClient));
			client.ReceivedRequest += Client_ReceivedRequest1;
			client.TryStart.CreateRun().RunToCompletion(true);
			Thread.Sleep(60000);
		}

		[TestMethod]
		public void TestMethod1()
		{
			var server = new Server(5007, "test server 1");
			server.TryStart.CreateRun().RunToCompletion();

			var client = new Client(5007, "test client 1");
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
			Server server = new(5007, "test server 2");
			Client client = new(5007, "test client 2");
			bool tested = false;
			RoR2Context expectedContext = new() { BossBodyName = Entities.Gup, IsBossEncounter = true, LoopIndex = 0, Survivor = Entities.Captain, SceneName = Scenes.SunderedGrove };
			server.ReceivedRequest += arg =>
			{
				foreach (var message in arg)
				{
					if (message.Key == "context")
					{
						var receivedContext = Json.FromJson<RoR2Context>(arg.Single().Value);

						foreach (var property in typeof(RoR2Context).GetProperties())
						{
							object? expectedValue = property.GetValue(expectedContext);
							object? actualValue = property.GetValue(receivedContext);
							if (!Equals(expectedValue, actualValue))
							{
								Debug.WriteLine($"{property.Name}: {expectedValue} != {actualValue}");
							}
						}

						tested = true;
					}
				}

				return Enumerable.Empty<Message>();
			};
			Assert.IsTrue(server.TryStart.CreateRun().Run(u => u.Args is not Exception).Result.Success);
			Assert.IsTrue(client.TryStart.CreateRun().Run(u => u.Args is not Exception).Result.Success);
			client.SendToServerAwaitResponse(new Message("context", Json.ToJson(expectedContext)));
			Assert.IsTrue(tested);
		}

		[TestMethod]
		public async Task IsThreadSafe()
		{
			Server server = new(5009, "test server 3");
			Client client = new(5009, "test client 3");

			server.TryStart.CreateRun().RunToCompletion();
			client.TryStart.CreateRun().RunToCompletion();

			server.ReceivedRequest += Server_ReceivedRequest;

			var task1 = client.SendToServerAsync(new Message("key 1", "value 1"));
			var task2 = client.SendToServerAsync(new Message("key 2", "value 2"));

			await task1;
			await task2;

			static IEnumerable<Message> Server_ReceivedRequest(IEnumerable<Message> arg)
			{
				Thread.Sleep(TimeSpan.FromSeconds(10));
				return arg.ToList();
			}
		}

		internal static IEnumerable<Message> Server_ReceivedRequest1(IEnumerable<Message> arg)
		{
			foreach (var message in arg)
			{
				Debug.Write(message);
			}

			yield break;
		}

		private IEnumerable<Message> Client_ReceivedRequest1(IEnumerable<Message> arg)
		{
			foreach (var message in arg)
			{
				this.Log(message);
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