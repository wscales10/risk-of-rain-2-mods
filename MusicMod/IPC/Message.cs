using System;

namespace IPC
{
	public class Message
	{
		public Message(string key, string value = null)
		{
			if (key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (key.Length > 4)
			{
				throw new ArgumentException("Key length must be no longer than 4");
			}

			Key = key;
			Value = value;
		}

		public string Key { get; }

		public string Value { get; }

		public static Message Parse(string s)
		{
			if (s is null)
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

		public sealed override string ToString() => Value is null ? Key : $"{Key} | {Value}";
	}
}