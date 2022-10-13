using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Helper;
using ZetaIpc.Runtime.Server;

namespace IPC
{
	public class Server : Utils.Runner
	{
		private readonly Dictionary<string, int> ports = new Dictionary<string, int>();

		private readonly Dictionary<string, IpcClient> clients = new Dictionary<string, IpcClient>();

		public Server(int port)
		{
			Port = port;
		}

		public event Func<IEnumerable<string>, IEnumerable<string>> ReceivedRequest;

		public int Port { get; }

		public IpcClient AddClient(string guid)
		{
			var client = new IpcClient();
			clients[guid] = client;
			client.Initialize(ports[guid]);
			return client;
		}

		public int GetPort(string guid)
		{
			int port = FreePortHelper.GetFreePort();
			ports[guid] = port;
			return port;
		}

		public void Broadcast(params string[] messages)
		{
			foreach (var client in clients.Values)
			{
				Methods.SendMessage(client, messages);
			}
		}

		protected override void Start()
		{
			var receiver = new IpcServer();
			receiver.Start(Port);

			receiver.ReceivedRequest += (s, e) =>
			{
				e.Response = Methods.Join(ReceivedRequest?.Invoke(Methods.Split(e.Request)));
				e.Handled = true;
			};

			_ = Task.Delay(Timeout.InfiniteTimeSpan);
		}
	}
}