using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Runners;

namespace IPC
{
	public abstract class Entity : CoroutineRunner
	{
		protected Entity(string description)
		{
			Description = description;
		}

		public event Func<IEnumerable<Message>, IEnumerable<Message>> ReceivedRequest;

		public virtual int? MyPort { get; protected set; }

		public string Description { get; }

		protected abstract string GuidString { get; }

		protected abstract Packet HandleReceivedPacket(Packet packet);

		protected string Receiver_ReceivedRequest(string arg)
		{
			Packet packet = Packet.ParseJson(arg);
			return HandleReceivedPacket(packet).ToString();
		}

		protected IEnumerable<Message> HandleReceivedRequest(IEnumerable<Message> messages)
		{
			return ReceivedRequest?.Invoke(messages) ?? Enumerable.Empty<Message>();
		}
	}
}