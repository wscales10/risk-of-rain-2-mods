using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Coroutines;

namespace IPC
{
	public class Client : Entity
	{
		private IClient sender;

		public Client(int serverPort)
		{
			ServerPort = serverPort;
			Guid = Guid.NewGuid();
			MyPort = null;
		}

		public event Action<IEnumerable<Message>> HandleConnResponse;

		public int ServerPort { get; }

		public Guid Guid { get; }

		protected override string GuidString => Guid.ToString();

		public Packet SendToServer(params Message[] messages)
		{
			return Methods.SendPacket(sender, MakePacket(messages));
		}

		public async Task<Packet> SendToServerAsync(params Message[] messages)
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

		protected override Packet HandleReceivedPacket(Packet packet)
		{
			Info.ThrowIfNotRunning();
			return MakePacket(HandleReceivedRequest(packet.Messages));
		}

		protected override IEnumerable<ProgressUpdate> Start(Reference reference)
		{
			sender = Manager.CreateClient();
			sender.Initialize(ServerPort);
			Packet portResponse = null;

			while (portResponse is null)
			{
				var response = SendToServerCatchWebException();

				switch (response)
				{
					case Packet packet:
						portResponse = packet;
						break;

					case Exception ex:
						yield return new ProgressUpdate(this, ex);
						break;

					default:
						throw new NotSupportedException();
				}
			}

			MyPort = portResponse.Port.Value;
			var receiver = Manager.CreateServer();
			receiver.Start(MyPort.Value);

			this.Log("Connecting");
			Packet connectionResponse = null;

			while (connectionResponse is null)
			{
				var response = SendToServerCatchWebException(new Message("conn"));

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

		private object SendToServerCatchWebException(params Message[] messages)
		{
			try
			{
				return SendToServer(messages);
			}
			catch (WebException ex)
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