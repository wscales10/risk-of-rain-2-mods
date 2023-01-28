using System;

namespace IPC
{
	public interface IServer
	{
		event Func<string, string> ReceivedRequest;

		int? Port { get; }

		void Start(int port);
	}
}