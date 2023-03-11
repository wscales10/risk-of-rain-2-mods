using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Coroutines;

namespace IPC
{
	public class Client : Entity
	{
		private ISender sender;

		private IReceiver receiver;

		public Client(int serverPort, string description) : base(description)
		{
			ServerPort = serverPort;
			Guid = Guid.NewGuid();
		}

		public event Action<IEnumerable<Message>> HandleConnResponse;

		public int ServerPort { get; }

		public Guid Guid { get; }

		public override int? MyPort { get => receiver.Port; protected set => throw new NotSupportedException(); }

		protected override string GuidString => Guid.ToString();

		public bool PingServer()
		{
			Info.ThrowIfNotRunning();
			Packet connectionResponse;
			try
			{
				connectionResponse = sendToServer(new Message("conn"));
			}
			catch (SendException)
			{
				return false;
			}

			this.Log(connectionResponse);

			this.Log("Handling Connection Response");
			HandleConnResponse?.Invoke(connectionResponse.Messages);
			this.Log("Handled Connection Response");
			return true;
		}

		public void SendToServer(params Message[] messages)
		{
			if (sender is IAsyncSender)
			{
				_ = sendToServerAsync(messages);
			}
			else
			{
				_ = sendToServer(messages);
			}
		}

		public Packet SendToServerAwaitResponse(params Message[] messages)
		{
			Info.ThrowIfNotRunning();
			return sendToServer(messages);
		}

		public async Task<Packet> SendToServerAwaitResponseAsync(params Message[] messages)
		{
			Info.ThrowIfNotRunning();
			if (sender is IAsyncSender)
			{
				return await sendToServerAsync(messages);
			}
			else
			{
				return sendToServer(messages);
			}
		}

		public async Task<Packet> SendToServerAsync(params Message[] messages)
		{
			Info.ThrowIfNotRunning();
			return await sendToServerAsync(messages);
		}

		protected override Packet HandleReceivedPacket(Packet packet)
		{
			Info.ThrowIfNotRunning();

			if (packet.ServerPort != ServerPort)
			{
				throw new DeliveryException(packet.ServerPort.Value, ServerPort);
			}

			return MakePacket(HandleReceivedRequest(packet.Messages));
		}

		protected override IEnumerable<ProgressUpdate> Start(Reference reference)
		{
			sender = Manager.CreateSender($"client of {Description ?? GetType().GetDisplayName()}");
			sender.Initialize(ServerPort);

			receiver = Manager.CreateReceiver();
			receiver.Start(0);
			this.Log($"receiver started on port {MyPort}: {Description}");

			this.Log("Connecting");
			Packet connectionResponse = null;

			while (connectionResponse is null)
			{
				SendException sendException;
				try
				{
					connectionResponse = sendToServer(new Message("conn"));
					break;
				}
				catch (SendException ex)
				{
					sendException = ex;
				}

				yield return new ProgressUpdate(this, sendException);
			}

			this.Log(connectionResponse);

			this.Log("Handling Connection Response");
			HandleConnResponse?.Invoke(connectionResponse.Messages);
			this.Log("Handled Connection Response");

			receiver.ReceivedRequest += Receiver_ReceivedRequest;

			_ = Task.Delay(Timeout.InfiniteTimeSpan);
			reference.Complete();
		}

		private async Task<Packet> sendToServerAsync(params Message[] messages)
		{
			if (sender is IAsyncSender asyncClient)
			{
				return await Methods.SendPacketAsync(asyncClient, MakePacket(messages));
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		private Packet sendToServer(params Message[] messages)
		{
			return Methods.SendPacket(sender, MakePacket(messages));
		}

		private Packet MakePacket(IEnumerable<Message> messages)
		{
			return new Packet(GuidString, MyPort, ServerPort, messages);
		}
	}
}