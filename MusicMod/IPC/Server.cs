using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Coroutines;

namespace IPC
{
	public class Server : Entity
	{
		private readonly Dictionary<string, int> strikes = new Dictionary<string, int>();

		private readonly PortHelper portHelper = new PortHelper();

		private readonly Dictionary<string, ISender> clients = new Dictionary<string, ISender>();

		public Server(int port, string description) : base(description)
		{
			MyPort = port;
		}

		public event Func<string, IEnumerable<Message>> OnAddClient;

		protected override string GuidString => null;

		public IEnumerable<Message> AddClient(string guid)
		{
			Info.ThrowIfNotRunning();

			var client = Manager.CreateSender($"client of {Description ?? GetType().GetDisplayName()}");
			clients[guid] = client;
			strikes[guid] = 0;
			client.Initialize(portHelper.GetPort(guid));
			return OnAddClient?.Invoke(guid) ?? Enumerable.Empty<Message>();
		}

		public void Broadcast(params Message[] messages)
		{
			Info.ThrowIfNotRunning();
			var guids = clients.Keys.ToList();

			foreach (var guid in guids)
			{
				sendToClientDynamic(guid, messages);
			}
		}

		public void SendToClient(string guid, params Message[] messages)
		{
			Info.ThrowIfNotRunning();
			sendToClientDynamic(guid, messages);
		}

		public async Task SendToClientAsync(string guid, params Message[] messages)
		{
			Info.ThrowIfNotRunning();

			var client = clients[guid];

			if (client is IAsyncSender asyncClient)
			{
				await sendToClientAsync(asyncClient, guid, messages);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		protected override IEnumerable<ProgressUpdate> Start(Reference reference)
		{
			var receiver = Manager.CreateReceiver();
			receiver.Start(MyPort.Value);

			receiver.ReceivedRequest += Receiver_ReceivedRequest;

			_ = Task.Delay(Timeout.InfiniteTimeSpan);
			reference.Complete();
			yield break;
		}

		protected override Packet HandleReceivedPacket(Packet packet)
		{
			Info.ThrowIfNotRunning();

			List<Message> response = new List<Message>();
			if (!portHelper.IsPortDefined(packet.Guid))
			{
				if (packet.Port is int port)
				{
					var toRemove = portHelper.GetGuidFromPort(port);

					if (!(toRemove is null) && toRemove != packet.Guid)
					{
						RemoveClient(toRemove);
					}

					if (!portHelper.SetPort(packet.Guid, port))
					{
						throw new SocketException();
					}

					response.AddRange(AddClient(packet.Guid));
				}
				else
				{
					throw new FormatException();
				}
			}
			else if (!clients.ContainsKey(packet.Guid))
			{
				response.AddRange(AddClient(packet.Guid));
			}

			var output = MakePacket(packet.Guid, response.Concat(HandleReceivedRequest(packet.Messages)));
			return output;
		}

		private void sendToClientDynamic(string guid, params Message[] messages)
		{
			var client = clients[guid];

			if (client is IAsyncSender asyncClient)
			{
				_ = sendToClientAsync(asyncClient, guid, messages);
			}
			else
			{
				sendToClient(client, guid, messages);
			}
		}

		private void sendToClient(ISender client, string guid, params Message[] messages)
		{
			try
			{
				Methods.SendPacket(client, MakePacket(guid, messages));
			}
			catch (SendException ex)
			{
				this.Log(ex);
				AddStrike(guid);
			}
			catch (Exception ex)
			{
				this.Log(ex);
				throw;
			}
		}

		private async Task sendToClientAsync(IAsyncSender asyncClient, string guid, params Message[] messages)
		{
			try
			{
				await Methods.SendPacketAsync(asyncClient, MakePacket(guid, messages));
			}
			catch (SendException ex)
			{
				this.Log(ex);
				AddStrike(guid);
			}
			catch (Exception ex)
			{
				this.Log(ex);
				throw;
			}
		}

		private void AddStrike(string guid)
		{
			if (++strikes[guid] > 2)
			{
				RemoveClient(guid);
			}
		}

		private void RemoveClient(string guid)
		{
			clients.Remove(guid);
			portHelper.Reset(guid);
			strikes.Remove(guid);
		}

		private Packet MakePacket(string guid, IEnumerable<Message> messages)
		{
			return new Packet(guid, portHelper.GetPort(guid), MyPort.Value, messages);
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
				try
				{
					return ports[guid];
				}
				catch (Exception)
				{
					System.Diagnostics.Debugger.Break();
					throw;
				}
			}

			public string GetGuidFromPort(int port)
			{
				return ports.SingleOrDefault(pair => pair.Value == port).Key;
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