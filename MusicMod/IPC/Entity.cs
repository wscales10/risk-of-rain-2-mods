using System;
using System.Collections.Generic;
using Utils;
using ZetaIpc.Runtime.Server;

namespace IPC
{
	public abstract class Entity : Runner
	{
		public event Func<IEnumerable<Message>, IEnumerable<Message>> ReceivedRequest;

		public int? MyPort { get; protected set; }

		protected abstract string GuidString { get; }

		protected abstract Packet HandleReceivedPacket(Packet packet);

		protected void Receiver_ReceivedRequest(object sender, ReceivedRequestEventArgs e)
		{
			e.Response = HandleReceivedPacket(Packet.Parse(e.Request)).ToString();
			e.Handled = true;
		}

		protected IEnumerable<Message> HandleReceivedRequest(IEnumerable<Message> messages)
		{
			return ReceivedRequest?.Invoke(messages);
		}
	}
}