using System.Collections.Generic;
using Utils;
using ZetaIpc.Runtime.Client;

namespace IPC
{
	public static class Methods
	{
		public static Packet SendPacket(IpcClient sender, Packet packet)
		{
			var outgoing = packet.ToString();
			sender.Log("***");
			sender.Log("Outgoing:");
			sender.Log(outgoing);
			var incoming = sender.Send(outgoing);
			sender.Log("Incoming:");
			sender.Log(incoming);
			sender.Log("");
			sender.Log("***");
			return Packet.Parse(incoming);
		}

		public static IEnumerable<string> Split(string joined)
		{
			return joined.Split(new[] { "\r\n" }, System.StringSplitOptions.None);
		}

		public static string Join(IEnumerable<string> messages)
		{
			return messages is null ? null : string.Join("\r\n", messages);
		}
	}
}