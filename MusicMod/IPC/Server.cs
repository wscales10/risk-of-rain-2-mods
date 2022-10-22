using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Coroutines;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Helper;
using ZetaIpc.Runtime.Server;

namespace IPC
{
	public class Server : Entity
	{
		private readonly Dictionary<string, int> strikes = new Dictionary<string, int>();

		private readonly PortHelper portHelper = new PortHelper();

		private readonly Dictionary<string, IpcClient> clients = new Dictionary<string, IpcClient>();

		public Server(int port)
		{
			MyPort = port;
		}

		public event Func<string, IEnumerable<Message>> OnAddClient;

		protected override string GuidString => null;

		public IEnumerable<Message> AddClient(string guid)
		{
			var client = new IpcClient();
			clients[guid] = client;
			strikes[guid] = 0;
			client.Initialize(portHelper.GetPort(guid));
			return OnAddClient?.Invoke(guid);
		}

		public void Broadcast(params Message[] messages)
		{
			foreach (var guid in clients.Keys)
			{
				SendToClient(guid, messages);
			}
		}

		public void SendToClient(string guid, params Message[] messages)
		{
			try
			{
				Methods.SendPacket(clients[guid], MakePacket(guid, messages));
			}
			catch (WebException ex)
			{
				this.Log(ex);
				AddStrike(guid);
			}
			catch
			{
				System.Diagnostics.Debugger.Break();
				throw;
			}
		}

		protected override CoroutineMethod Start()
		{
			return reference =>
			{
				var receiver = new IpcServer();
				receiver.Start(MyPort.Value);

				receiver.ReceivedRequest += Receiver_ReceivedRequest;

				_ = Task.Delay(Timeout.InfiniteTimeSpan);
				return new object[] { };
			};
		}

		protected override Packet HandleReceivedPacket(Packet packet)
		{
			List<Message> response = new List<Message>();
			if (!portHelper.IsPortDefined(packet.Guid))
			{
				if (packet.Port is int port)
				{
					portHelper.SetPort(packet.Guid, port);
					response.AddRange(AddClient(packet.Guid));
				}
				else
				{
					portHelper.SetPort(packet.Guid);
				}
			}
			else if (!clients.ContainsKey(packet.Guid))
			{
				response.AddRange(AddClient(packet.Guid));
			}

			var output = MakePacket(packet.Guid, response.Concat(HandleReceivedRequest(packet.Messages)));
			return output;
		}

		private void AddStrike(string guid)
		{
			if (++strikes[guid] > 2)
			{
				clients.Remove(guid);
				portHelper.Reset(guid);
				strikes.Remove(guid);
			}
		}

		private Packet MakePacket(string guid, IEnumerable<Message> messages)
		{
			return new Packet(guid, portHelper.GetPort(guid), messages);
		}

		private sealed class PortHelper
		{
			private readonly Dictionary<string, int> ports = new Dictionary<string, int>();

			public bool IsPortDefined(string guid)
			{
				return ports.ContainsKey(guid);
			}

			public int GetPort(string guid)
			{
				return ports[guid];
			}

			public void SetPort(string guid)
			{
				for (int i = 0; i < 5; i++)
				{
					if (SetPort(guid, FreePortHelper.GetFreePort()))
					{
						return;
					}
				}

				throw new NotImplementedException("Failed to find valid port");
			}

			public bool SetPort(string guid, int port)
			{
				if (ports.Any(pair => pair.Value == port && pair.Key != guid))
				{
					return false;
				}

				ports[guid] = port;
				return true;
			}

			public void Reset(string guid)
			{
				ports.Remove(guid);
			}
		}
	}
}