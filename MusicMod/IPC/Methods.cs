using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;

namespace IPC
{
	public static class Methods
	{
		public static Packet SendPacket(ISender sender, Packet packet)
		{
			string outgoing = packet.WithSenderDescription(sender.Description).ToString();
			sender.Log("***");
			sender.Log("Outgoing:");
			sender.Log(outgoing);
			var incoming = sender.Send(outgoing);
			sender.Log("Incoming:");
			sender.Log(incoming);
			sender.Log("");
			sender.Log("***");
			return Packet.ParseJson(incoming);
		}

		public static async Task<Packet> SendPacketAsync(IAsyncSender sender, Packet packet)
		{
			var outgoing = packet.WithSenderDescription(sender.Description).ToString();
			sender.Log("***");
			sender.Log("Outgoing:");
			sender.Log(outgoing);
			var incoming = await sender.SendAsync(outgoing);
			sender.Log("Incoming:");
			sender.Log(incoming);
			sender.Log("");
			sender.Log("***");
			return Packet.ParseJson(incoming);
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