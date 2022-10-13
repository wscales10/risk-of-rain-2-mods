using System.Collections.Generic;
using ZetaIpc.Runtime.Client;

namespace IPC
{
	public static class Methods
	{
		public static IEnumerable<string> SendMessage(IpcClient sender, params string[] messages)
		{
			return Split(sender.Send(Join(messages)));
		}

		public static IEnumerable<string> Split(string joined)
		{
			return joined.Split(new[] { "\r\n" }, System.StringSplitOptions.None);
		}

		public static string Join(IEnumerable<string> messages)
		{
			return string.Join("\r\n", messages);
		}
	}
}