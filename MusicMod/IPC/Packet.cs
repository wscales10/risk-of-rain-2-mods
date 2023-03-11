using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace IPC
{
	public class Packet
	{
		public Packet(string guid, int? port, int serverPort, IEnumerable<Message> messages)
		{
			Guid = guid;
			Port = port;
			ServerPort = serverPort;
			Messages = messages.ToReadOnlyCollection();
		}

		public string SenderDescription { get; private set; }

		public int? ServerPort { get; private set; }

		public string Guid { get; }

		public int? Port { get; }

		public IReadOnlyCollection<Message> Messages { get; }

		public static Packet ParseJson(string json) => Json.FromJson<Packet>(json);

		public static Packet ParseOldString(string s)
		{
			var lines = Methods.Split(s);
			var messages = lines.Select(Message.ParseJson).ToList();

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

			return new Packet(guidMessage.Value, portMessage?.Value is null ? (int?)null : int.Parse(portMessage.Value), 0, messages);
		}

		public Packet WithSenderDescription(string senderDescription)
		{
			if (SenderDescription is null)
			{
				SenderDescription = senderDescription;
			}
			else
			{
				throw new InvalidOperationException("Cannot set sender description multiple times");
			}

			return this;
		}

		public sealed override string ToString() => Json.ToJson(this);

		public string AsString() => Methods.Join(new[] { new Message("guid", Guid), new Message("port", Port?.ToString()) }.Concat(Messages).Select(m => m.ToString()));
	}
}