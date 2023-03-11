using System;

namespace IPC
{
	public interface IReceiver
	{
		event Func<string, string> ReceivedRequest;

		int? Port { get; }

		void Start(int port);
	}
}