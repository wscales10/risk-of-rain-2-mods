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
		private IClient sender;

		private IServer receiver;

		public Client(int serverPort)
		{
			ServerPort = serverPort;
			Guid = Guid.NewGuid();
		}

		public event Action<IEnumerable<Message>> HandleConnResponse;

		public int ServerPort { get; }

		public Guid Guid { get; }

		public override int? MyPort { get => receiver.Port; protected set => throw new NotSupportedException(); }

		protected override string GuidString => Guid.ToString();

		public void SendToServer(params Message[] messages)
		{
			if (sender is IAsyncClient)
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
			if (sender is IAsyncClient)
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
			return MakePacket(HandleReceivedRequest(packet.Messages));
		}

		protected override IEnumerable<ProgressUpdate> Start(Reference reference)
		{
			sender = Manager.CreateClient();
			sender.Initialize(ServerPort);

			receiver = Manager.CreateServer();
			receiver.Start(0);

			this.Log("Connecting");
			Packet connectionResponse = null;

			while (connectionResponse is null)
			{
				var response = SendToServerCatchSendException(new Message("conn"));

				switch (response)
				{
					case Packet packet:
						connectionResponse = packet;
						break;

					case Exception ex:
						yield return new ProgressUpdate(this, ex);
						break;

					default:
						throw new NotSupportedException();
				}
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
			if (sender is IAsyncClient asyncClient)
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

		private object SendToServerCatchSendException(params Message[] messages)
		{
			try
			{
				return sendToServer(messages);
			}
			catch (SendException ex)
			{
				return ex;
			}
		}

		private Packet MakePacket(IEnumerable<Message> messages)
		{
			return new Packet(GuidString, MyPort, messages);
		}
	}
}