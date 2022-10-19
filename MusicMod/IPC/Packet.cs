using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace IPC
{
	public class Packet
	{
		public Packet(string guid, int? port, IEnumerable<Message> messages)
		{
			Guid = guid;
			Port = port;
			Messages = messages.ToReadOnlyCollection();
		}

		public string Guid { get; }

		public int? Port { get; }

		public IReadOnlyCollection<Message> Messages { get; }

		public static Packet Parse(string s)
		{
			var lines = Methods.Split(s);
			var messages = lines.Select(Message.Parse).ToList();

			Message guidMessage = null, portMessage = null;
			messages = messages.Where(message =>
			{
				switch (message.Key)
				{
					case "guid":
						if (guidMessage is null)
						{
							guidMessage = message;
							return false;
						}
						else
						{
							throw new FormatException();
						}

					case "port":
						if (portMessage is null)
						{
							portMessage = message;
							return false;
						}
						else
						{
							throw new FormatException();
						}
					default:
						return true;
				}
			}).ToList();

			return new Packet(guidMessage.Value, portMessage?.Value is null ? (int?)null : int.Parse(portMessage.Value), messages);
		}

		public sealed override string ToString() => Methods.Join(new[] { new Message("guid", Guid), new Message("port", Port?.ToString()) }.Concat(Messages).Select(m => m.ToString()));
	}
}