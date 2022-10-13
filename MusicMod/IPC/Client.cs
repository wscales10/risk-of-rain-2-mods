using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Server;

namespace IPC
{
	public class Client : Runner
	{
		private IpcClient sender;

		public Client(int serverPort)
		{
			ServerPort = serverPort;
			Guid = Guid.NewGuid();
		}

		public event Action<IEnumerable<string>> HandleConnResponse;

		public event Func<IEnumerable<string>, IEnumerable<string>> ReceivedRequest;

		public int ServerPort { get; }

		public Guid Guid { get; }

		public void SendToServer(params string[] messages)
		{
			Methods.SendMessage(sender, messages);
		}

		protected override void Start()
		{
			try
			{
				sender = new IpcClient();
				sender.Initialize(ServerPort);
				var portResponse = Methods.SendMessage(sender, $"port | {Guid}").Single();

				if (portResponse.Substring(0, 4) != "port")
				{
					throw new NotSupportedException();
				}

				int port = int.Parse(portResponse.Substring(7));
				var receiver = new IpcServer();
				receiver.Start(port);

				HandleConnResponse?.Invoke(Methods.SendMessage(sender, $"conn | {Guid}"));

				receiver.ReceivedRequest += (s, e) =>
				{
					e.Response = Methods.Join(ReceivedRequest?.Invoke(Methods.Split(e.Request)));
					e.Handled = true;
				};

				_ = Task.Delay(Timeout.InfiniteTimeSpan);
			}
			catch (Exception ex)
			{
				this.Log(ex);
				System.Diagnostics.Debugger.Break();
				throw;
			}
		}
	}
}