using System;
using Utils;

namespace IPC
{
	public class Message
	{
		private readonly string key;

		private readonly string value;

		public Message(string key, string value = null)
		{
			this.key = key ?? throw new ArgumentNullException(nameof(key));
			this.value = value;
		}

		public string Key => key;

		public string Value => value;

		public static Message ParseJson(string json) => Json.FromJson<Message>(json);

		public static Message ParseOneLineString(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException(nameof(s));
			}

			int index = s.IndexOf(" |");

			if (index == -1)
			{
				if (s.Length < 5)
				{
					return new Message(s, null);
				}
			}
			else if (index < 5)
			{
				if (s.Length < 5)
				{
					return new Message(s.Substring(0, index), null);
				}
				else if (s[6] == ' ')
				{
					return new Message(s.Substring(0, index), s.Substring(7));
				}
			}

			throw new FormatException();
		}

		public string ToOneLineString()
		{
			return Value is null ? Key : $"{Key} | {Value}";
		}

		public sealed override string ToString() => Json.ToJson(this);
	}
}