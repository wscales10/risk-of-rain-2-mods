using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Server;

namespace IPC
{
	public class Client : Entity
	{
		private IpcClient sender;

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

		protected override Packet HandleReceivedPacket(Packet packet)
		{
			return MakePacket(HandleReceivedRequest(packet.Messages));
		}

		protected override void Start()
		{
			try
			{
				sender = new IpcClient();
				sender.Initialize(ServerPort);
				var portResponse = SendToServerIgnoreTimeout();

				MyPort = portResponse.Port.Value;
				var receiver = new IpcServer();
				receiver.Start(MyPort.Value);

				this.Log("Connecting");
				var connectionResponse = SendToServerIgnoreTimeout(new Message("conn"));
				this.Log(connectionResponse);

				this.Log("Handling Connection Response");
				HandleConnResponse?.Invoke(connectionResponse.Messages);
				this.Log("Handled Connection Response");

				receiver.ReceivedRequest += Receiver_ReceivedRequest;

				_ = Task.Delay(Timeout.InfiniteTimeSpan);
			}
			catch (Exception ex)
			{
				this.Log(ex);
				System.Diagnostics.Debugger.Break();
				throw;
			}
		}

		private Packet SendToServerIgnoreTimeout(params Message[] messages)
		{
			while (true)
			{
				try
				{
					return SendToServer(messages);
				}
				catch (WebException ex)
				{
					switch (ex.Status)
					{
						case WebExceptionStatus.Timeout:
							break;

						case WebExceptionStatus.ConnectFailure:
						case WebExceptionStatus.UnknownError:
							Thread.Sleep(1000);
							break;

						default:
							throw;
					}

					this.Log(ex);
				}
			}
		}

		private Packet MakePacket(IEnumerable<Message> messages)
		{
			return new Packet(GuidString, MyPort, messages);
		}
	}
}